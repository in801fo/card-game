using UnityEngine;
using Discord.Sdk;

public class RichPresenceHandler : MonoBehaviour
{
    private const string inALobbyHeading = "In a Lobby";

    private const string inALobbyAwaitingConnectionState = "In a lobby awaiting to connect";

    private const string inGameHeading = "In a match";

    private const string inGameState = "In a match";

    private string currentDetails;

    private string currentState;

    private ulong startTimestamp;

    private void Start()
    {
        startTimestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        GameManager.OnDoneGenerating += () => UpdateRichPresence(inGameState, inGameHeading);
    }

    public void UpdateRichPresence(string details, string heading)
    {
        currentDetails = heading;
        currentState = details;
        UpdateRichPresence(DiscordManager.client);
    }

    public void UpdateRichPresence(Client client)
    {
        Activity activity = new Activity();

        activity.SetType(ActivityTypes.Playing);
        activity.SetDetails(currentDetails);
        activity.SetState(currentState);

        var activityTimestamp = new ActivityTimestamps();
        activityTimestamp.SetStart(startTimestamp);
        activity.SetTimestamps(activityTimestamp);

        client.UpdateRichPresence(activity, OnUpdateRichPresence);
    }

    private void OnUpdateRichPresence(ClientResult result)
    {
        if (result.Successful())
        {
            Debug.Log("Rich presence updated!");
        }
        else
        {
            Debug.LogError($"Failed to update rich presence {result.Error()}");
        }
    }
}