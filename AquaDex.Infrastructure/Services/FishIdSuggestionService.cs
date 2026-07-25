using AquaDex.Core.Entities;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace AquaDex.Infrastructure.Services;

public class FishIdSuggestionService
{
    private readonly ChatClient _chatClient;

    public FishIdSuggestionService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured.");
        _chatClient = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
    }

    public async Task<string> SuggestSpeciesAsync(string threadTitle, string threadBody, List<Species> knownSpecies)
    {
        var speciesListText = string.Join("\n", knownSpecies.Select(s =>
            $"- {s.CommonNameEn} ({s.LatinName}): {s.HabitatType}, conservation status {s.ConservationStatus}"));

        var prompt = $"""
            A user posted this fish identification request on a fisheries forum for Azerbaijan:

            Title: {threadTitle}
            Description: {threadBody}

            Here are species known to be in the AquaDex databank:
            {speciesListText}

            Based on the description only (no image was provided), suggest which species from the list above is the most likely match, or say if you cannot determine one confidently from text alone. Keep your answer to 2-3 sentences. If the description suggests a protected or endangered species, mention that the user should treat it with caution and consult a Verified Expert before handling or catching it.
            """;

        var completion = await _chatClient.CompleteChatAsync(prompt);
        return completion.Value.Content[0].Text;
    }
}