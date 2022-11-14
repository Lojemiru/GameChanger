
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.CommandLine;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

class Program
{
    static async Task<int> Main(string[] args)
    {

        var pathOption = new Option<DirectoryInfo>(
            name: "--input",
            description: "The folder containing the .yy file to convert."
            );

        var outputOption = new Option<DirectoryInfo>(
            name: "--output",
            description: "The folder to put the converted data into."
            );

        var rootCommand = new RootCommand("Asset porting utility to save people from the woes of GameMaker Studio 2.");

        var spriteCommand = new Command("sprite", "Convert a .yy sprite into JSON metadata and individual frames.");
        spriteCommand.AddOption(pathOption);
        spriteCommand.AddOption(outputOption);

        rootCommand.AddCommand(spriteCommand);

        spriteCommand.SetHandler((file, output) =>
        {
            ConvertSprite(file, output);
        },
        pathOption, outputOption);

        return await rootCommand.InvokeAsync(args);
    }

    static void ConvertSprite(DirectoryInfo inputDir, DirectoryInfo outputDir)
    {
        string filename = inputDir.FullName + "/" + inputDir.Name + ".yy";
        YYSprite sprite = YYSprite.FromJson(File.ReadAllText(filename));

        //Console.WriteLine(sprite.Frames[0].Name);

        if (Directory.Exists(outputDir.FullName + "/" + sprite.Name))
            Directory.Delete(outputDir.FullName + "/" + sprite.Name, true);

        DirectoryInfo output = Directory.CreateDirectory(outputDir.FullName + "/" + sprite.Name);

        for (var i = 0; i < sprite.Frames.Length; i++)
        {
            string name = sprite.Frames[i].Name;
            File.Copy(inputDir + "/" + name + ".png", output.FullName + "/" + i + ".png");
        }

        File.WriteAllText(output + "/origin.txt", sprite.Sequence.OriginX + "\n" + sprite.Sequence.OriginY);
    }
}


public class YYSprite
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("width")]
    public int Width { get; set; }
    [JsonProperty("height")]
    public int Height { get; set; }
    [JsonProperty("frames")]
    public Frame[] Frames { get; set; }
    [JsonProperty("sequence")]
    public Sequence Sequence { get; set; }

    public static YYSprite FromJson(string json) => JsonConvert.DeserializeObject<YYSprite>(json, Converter.Settings);
}

public class Sequence
{
    [JsonProperty("xorigin")]
    public int OriginX { get; set; }
    [JsonProperty("yorigin")]
    public int OriginY { get; set; }

    public static Sequence FromJson(string json) => JsonConvert.DeserializeObject<Sequence>(json, Converter.Settings);
}

public class Frame
{
    [JsonProperty("name")]
    public string Name { get; set; }

    public static Frame FromJson(string json) => JsonConvert.DeserializeObject<Frame>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this YYSprite self) => JsonConvert.SerializeObject(self, Converter.Settings);
    public static string ToJson(this Sequence self) => JsonConvert.SerializeObject(self, Converter.Settings);
    public static string ToJson(this Frame self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
}