using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ChatBot_Demo;
public sealed class ChatbotConfiguration
{
    // Deployment Parameters
    public required string DeploymentName { get; set; }

    // Azure Cognitive Services Parameters
    public required string AzureCognitiveServicesEndpoint { get; set; }
    public required string AzureCognitiveServicesKey { get; set; }

    // Search Service Parameters
    public required string AzureQuestionAnsweringSearchServiceEndpoint { get; set; }
    public required string AzureQuestionAnsweringSearchServiceKey { get; set; }
}

public sealed class QnAConfiguration
{
    public required IList<QnAValue> qavalues { get; set; }
}

public sealed class QnAValue
{
    public required string op { get; set; }
    public required QnA value { get; set; }
}

public sealed class QnA
{
    public int id { get; set; }
    public required IList<string> questions { get; set; }
    public required string answer { get; set; }
    public string source { get; set; }
    public Dictionary<string,string> metadata { get; set; }
    public Dialog dialog { get; set; }
}

public sealed class Dialog
{
    public string prompt { get; set; }
    public int displayOrder { get; set; }
}