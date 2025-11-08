namespace FunnyActivities.Domain.Enums
{
    /// <summary>
    /// Represents the supported activity video asset types.
    /// </summary>
    public enum ActivityVideoType
    {
        /// <summary>
        /// Main instructional video that drives the activity steps.
        /// </summary>
        Main = 0,

        /// <summary>
        /// Optional intro video that plays before the main activity video.
        /// </summary>
        Intro = 1
    }
}
