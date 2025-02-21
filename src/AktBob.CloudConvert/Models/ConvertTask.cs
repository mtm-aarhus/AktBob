using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models;
internal class ConvertTask
{
    [JsonPropertyName("operation")]
    public string Operation { get; } = "convert";

    [JsonPropertyName("input")]
    public string[] Input { get; set; } = Array.Empty<string>();

    [JsonPropertyName("input_format")]
    public string InputFormat { get; } = "html";

    [JsonPropertyName("output_format")]
    public string OutputFormat { get; } = "pdf";

    [JsonPropertyName("engine")]
    public string Engine { get; } = "chrome";

    [JsonPropertyName("zoom")]
    public int Zoom { get; } = 1;

    [JsonPropertyName("page_orientation")]
    public string PageOrientation { get; } = "portrait";

    [JsonPropertyName("print_background")]
    public bool PrintBackground { get; } = false;

    [JsonPropertyName("display_header_footer")]
    public bool DisplayHeaderFooter { get; } = false;

    [JsonPropertyName("wait_until")]
    public string WaitUntil { get; } = "load";

    [JsonPropertyName("wait_time")]
    public int WaitTime { get; } = 0;

    [JsonPropertyName("page_format")]
    public string PageFormat { get; } = "a4";

    [JsonPropertyName("margin_top")]
    public int MarginTop { get; } = 10;

    [JsonPropertyName("margin_bottom")]
    public int MarginBottom { get; } = 15;

    [JsonPropertyName("margin_left")]
    public int MarginLeft { get; } = 15;

    [JsonPropertyName("margin_right")]
    public int MarginRight { get; } = 15;
}