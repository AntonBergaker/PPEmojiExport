
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

var targetPath = "./output";
var sourcePath = "../../../../twemoji/assets/72x72";

var imagePaths = new Stack<string>(Directory.GetFiles(sourcePath));

int imageSize = 72;
int padding = 2;
int pageSize = 2048;

int emojiOnRow = pageSize / (imageSize + padding);

if (!Directory.Exists(targetPath)) {
    Directory.CreateDirectory(targetPath);
}

var locations = new Dictionary<string, Location>();
int images = 0;

while (imagePaths.Count > 0) {

    Image<Rgba32> image = new Image<Rgba32>(pageSize, pageSize, Rgba32.ParseHex("00000000"));
    image.Mutate(o => {
        for (int y = 0; y < emojiOnRow; y++) {
            for (int x = 0; x < emojiOnRow; x++) {
                if (imagePaths.Count == 0) {
                    goto outer_break;
                }

                var path = imagePaths.Pop();
                var emoji = Image.Load<Rgba32>(path);
                var position = new Point(x, y) * (imageSize + padding);
                o.DrawImage(emoji, position, new GraphicsOptions());
                locations.Add(Path.GetFileNameWithoutExtension(path), new() {
                    Page = images,
                    X = position.X,
                    Y = position.Y,
                });
            }
        }
        outer_break: ;
    });

    image.Save(Path.Join(targetPath, $"image{images++}.png"));
}

var txt = JsonSerializer.Serialize(locations);

File.WriteAllText(
    Path.Join(targetPath, "locations.json"),
    txt
);

File.WriteAllText(
    Path.Join(targetPath, "scr_emoji_get_location_json_string.gml"),
    "function scr_emoji_get_location_json_string() { return \"" + txt.Replace("\"", "\\\"") + " \"; }"
);

record Location {
    public int Page { init; get; }
    public int X { init; get; }
    public int Y { init; get; }
}