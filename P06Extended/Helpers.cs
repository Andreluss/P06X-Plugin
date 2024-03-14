namespace P06X.Helpers
{
    using Rewired;

    public class XInput
    {
        // Thanks to Beatz for the original (InputHelper) code (^^)!
        public const string REWIRED_A = "Button A";

        public const string REWIRED_B = "Button B";

        public const string REWIRED_X = "Button X";

        public const string REWIRED_Y = "Button Y";

        public const string REWIRED_LS_X = "Left Stick X";

        public const string REWIRED_LS_Y = "Left Stick Y";

        public const string REWIRED_RS_X = "Right Stick X";

        public const string REWIRED_RS_Y = "Right Stick Y";

        public const string REWIRED_DPAD_X = "D-Pad X";

        public const string REWIRED_DPAD_Y = "D-Pad Y";

        public const string REWIRED_START = "Start";

        public const string REWIRED_BACK = "Back";

        public const string REWIRED_RIGHT_TRIGGER = "Right Trigger";

        public const string REWIRED_RIGHT_BUMPER = "Right Bumper";

        public const string REWIRED_LEFT_TRIGGER = "Left Trigger";

        public const string REWIRED_LEFT_BUMPER = "Left Bumper";

        public static Player Controls => Singleton<RInput>.Instance.GetFieldValue<Player>("P");

        public static bool IsControlAxisPastThreshold(string analog, string digital, double threshold)
        {
            Player rewiredPlayer = Controls;
            if (threshold > 0.0)
            {
                return (double)rewiredPlayer.GetAxis(analog) > threshold || (double)rewiredPlayer.GetAxis(digital) > threshold;
            }

            return (double)rewiredPlayer.GetAxis(analog) < threshold || (double)rewiredPlayer.GetAxis(digital) < threshold;
        }

        public static bool IsControlXAxisPastThreshold(double threshold)
        {
            return IsControlAxisPastThreshold("Left Stick X", "D-Pad X", threshold);
        }

        public static bool IsControlYAxisPastThreshold(double threshold)
        {
            return IsControlAxisPastThreshold("Left Stick Y", "D-Pad Y", threshold);
        }

        public static bool IsControlRightStickX(double threshold)
        {
            return IsControlAxisPastThreshold("Right Stick X", string.Empty, threshold);
        }
    }
}
