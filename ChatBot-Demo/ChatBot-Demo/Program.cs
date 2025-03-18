using Azure;
using Azure.Core;
using Azure.AI.TextAnalytics;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.QuestionAnswering.Authoring;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using ChatBot_Demo;
using System.Diagnostics;



// Get the endpoint and API cognitiveServicesKey from the application configuration

IConfigurationRoot builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("qna.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

ChatbotConfiguration? configuration = builder.GetRequiredSection("Configuration").Get<ChatbotConfiguration>();
if (configuration == null)
{
    Console.WriteLine("Failed to load the appsettings.json file.");
    return;
}
Uri cognitiveServicesEndpoint = new Uri(configuration.AzureCognitiveServicesEndpoint);
AzureKeyCredential cognitiveServicesKey = new AzureKeyCredential(configuration.AzureCognitiveServicesKey);
string chatDeploymentName = configuration.DeploymentName;

TextAnalyticsClient textAnalyticsClient = new TextAnalyticsClient(cognitiveServicesEndpoint, cognitiveServicesKey);
QuestionAnsweringAuthoringClient client = new QuestionAnsweringAuthoringClient(cognitiveServicesEndpoint, cognitiveServicesKey);
QuestionAnsweringClient questionAnsweringClient = new QuestionAnsweringClient(cognitiveServicesEndpoint, cognitiveServicesKey);

// Create a new Question Answering project
using RequestContent projectCreationRequestContent = RequestContent.Create(new
    {
        description = "Chatbot demo project",
        language = "en",
        multilingualResource = false,
        settings = new
        {
            defaultAnswer = "Sorry, I couldn't understand that."
        }
    }
);

Response projectCreationResponse = await client.CreateProjectAsync(chatDeploymentName, projectCreationRequestContent);
if (projectCreationResponse.Status != 200 && projectCreationResponse.Status != 201)
{
    Console.WriteLine($"Failed to create the project. Status code: {projectCreationResponse.Status}");
    return;
}
JsonElement jsonElement = JsonDocument.Parse(projectCreationResponse.ContentStream).RootElement;
Console.WriteLine($"Project created successfully. Project ID: {jsonElement.GetProperty("projectName").ToString()}");
try
{
    Console.WriteLine("Creating the QnAs for the Chatbot...");
    // Create QnAs for the Chatbot
    QnAConfiguration? qnaConfiguration = builder.GetRequiredSection("qna").Get<QnAConfiguration>();
    if (qnaConfiguration == null)
    {
        Console.WriteLine("Failed to load the qna.json file.");
        return;
    }

    // Create QnAs for the Chatbot
    using RequestContent qNAList = RequestContent.Create(qnaConfiguration.qavalues);

    var qNAContentOperation = await client.UpdateQnasAsync(WaitUntil.Completed, chatDeploymentName, qNAList);
    var qNAContentResponse = await qNAContentOperation.WaitForCompletionAsync();
    if (qNAContentResponse.GetRawResponse().Status != 200 && qNAContentResponse.GetRawResponse().Status != 201)
    {
        Console.WriteLine($"Failed to create the QnAs. Status code: {qNAContentResponse.GetRawResponse().Status}");
        return;
    }
    Console.WriteLine("QnAs created successfully.");

    await foreach (var qna in client.GetQnasAsync(chatDeploymentName))
    {
        JsonElement parsedResponse = JsonDocument.Parse(qna.ToStream()).RootElement;
        Debug.WriteLine($"QnA ID: {parsedResponse.GetProperty("id")}, Questions: {string.Join(", ", parsedResponse.GetProperty("questions"))}");
    }

    Console.WriteLine("Deploying the project...");
    // Set deployment name and start operation
    Operation<BinaryData> deploymentOperation = client.DeployProject(WaitUntil.Completed, chatDeploymentName, "production");

    // Retrieve deployments
    Pageable<BinaryData> deployments = client.GetDeployments(chatDeploymentName);
    Debug.WriteLine("Deployments: ");
    foreach (BinaryData deployment in deployments)
    {
        Debug.WriteLine(deployment);
    }
    Console.WriteLine($"Deployment completed.");

    // Ask a question
    QuestionAnsweringProject project = new QuestionAnsweringProject(chatDeploymentName, chatDeploymentName);

    Console.WriteLine($"Question: Hello, how are you?");
    Response<AnswersResult> response = await questionAnsweringClient.GetAnswersAsync("Hello, how are you?", project);
    if (response.Value.Answers.Count > 0)
    {
        Console.WriteLine($"Answer: {response.Value.Answers.First().Answer}");
    }

    Console.WriteLine($"Question: What is GlauxServices?");
    response = await questionAnsweringClient.GetAnswersAsync("What is GlauxServices?", project);
    if (response.Value.Answers.Count > 0)
    {
        Console.WriteLine($"Answer: {response.Value.Answers.First().Answer}");
    }

    Console.WriteLine($"Question: How do I contact support?");
    response = await questionAnsweringClient.GetAnswersAsync("How do I contact support?", project);
    if (response.Value.Answers.Count > 0)
    {
        Console.WriteLine($"Answer: {response.Value.Answers.First().Answer}");
    }

    Console.WriteLine("Question: Is this Invalid?");
    response = await questionAnsweringClient.GetAnswersAsync("Is this Invalid?", project);
    if (response.Value.Answers.Count > 0)
    {
        Console.WriteLine($"Answer: {response.Value.Answers.First().Answer}");
    }

    Console.WriteLine("Opening Console input. Please ask a question or type 'q' to quit");
    while (true)
    {
        Console.Write("Question: ");
        string? question = Console.ReadLine();
        if (question == "q")
        {
            break;
        }
        if (String.IsNullOrEmpty(question))
        {
            Console.WriteLine("Please enter a question.");
            continue;
        }
        response = await questionAnsweringClient.GetAnswersAsync(question, project);
        if (response.Value.Answers.Count > 0)
        {
            Console.WriteLine($"Answer: {response.Value.Answers.First().Answer}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error in excution: {ex.Message}");
}
finally
{

    // Delete the project
    Console.WriteLine("Deleting the project...");
    var deleteOperation = await client.DeleteProjectAsync(WaitUntil.Completed, chatDeploymentName);

    var deleteResponse = await deleteOperation.WaitForCompletionResponseAsync();
    Console.WriteLine($"Project Deletion - Status: {deleteResponse.Status}");

}