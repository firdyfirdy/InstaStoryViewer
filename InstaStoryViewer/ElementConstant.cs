namespace InstaStoryViewer;

public abstract class ElementConstant
{
    public const string NotificationPopup = "button[tabindex=\"0\"]:nth-child(2)";
    public const string StoriesCheck = "li[tabindex=\"-1\"] button:nth-child(1)";
    public const string StoriesNext = "button[aria-label=\"Next\"]";
    public const string LikeStory = "svg[aria-label=\"Like\"]";
}