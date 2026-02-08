using UnityEngine;
using Discord.Sdk;

public class RichPresenceHandler : MonoBehaviour
{
    [SerializeField] private string details = "In Unity";

    [SerializeField] private string state = "Building a game (Ciao Kim)";

    private ulong startTimestamp;

    private void Start()
    {
        startTimestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public void UpdateRichPresence(Client client)
    {
        Activity activity = new Activity();

        activity.SetType(ActivityTypes.Playing);
        activity.SetDetails(details);
        activity.SetState(state);

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