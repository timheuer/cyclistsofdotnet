using Mustache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

string rootDir = args.Length > 0 ? args[0] : "..\\.";

var template = File.ReadAllText(Path.Combine(rootDir, "template.html"));
var cyclistData = JsonConvert.DeserializeObject<Cyclist[]>(File.ReadAllText(Path.Combine(rootDir, "cyclists.json"))).OrderBy(p => p.name);

var rendered = Template.Compile(template).Render(new { cyclists = await makeBase64Encoded(cyclistData) });
File.WriteAllText(Path.Combine(rootDir, "index.html"), rendered);

async static Task<IEnumerable<Cyclist>> makeBase64Encoded(IEnumerable<Cyclist> cyclists)
{
    var client = new HttpClient();
    var b64Cyclists = new List<Cyclist>();
    foreach (var cyclist in cyclists)
    {
        string b64 = string.Empty;
        try
        {
            var imgData = await client.GetAsync(cyclist.img);
            b64 = Convert.ToBase64String(await imgData.Content.ReadAsByteArrayAsync());
        }
        finally
        {
            b64Cyclists.Add(cyclist with { img = "data:image/jpeg;base64," + b64 });
        }
    }
    return b64Cyclists;
}

record Cyclist(string name, string jobTitle, string img, string strava, string twitter, string github);