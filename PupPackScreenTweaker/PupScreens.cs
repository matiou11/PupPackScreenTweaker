using System.Collections.Generic;

namespace PupPackScreenTweaker
{
    public class PupScreens : List<PupScreen>
    {
        public const int FIRST_USER_SCREENINDEX = 11;
        public const int MAX_ALLOWED_SCREENINDEX = 19;
        public const int SPECIAL_99_SCREENINDEX = 99;
        public const int BACKGLASS_SCREENINDEX = 2;
        public const string DEFAULT_ACTIVE_MODE = "show";
        public const string DEFAULT_SCREEN_CAPTION = "PUP SCREEN #";

        public PupScreens():base()
        {
        
        }

        public bool DoesScreenIndexExists(int index)
        {
            foreach (PupScreen ps in this) if (ps.ScreenIndex == index) return true;
            return false;
        }
        public int GetNextAvailableCustomIndex()
        {
            for (int nIndex = FIRST_USER_SCREENINDEX; nIndex <= MAX_ALLOWED_SCREENINDEX; nIndex++)
            {
                if (!DoesScreenIndexExists(nIndex)) return nIndex;
            }
            return -1;
        }

        public PupScreen AddOne(int screenIndex, bool transparent, PupScreens refScreens)
        {
            PupScreen ps = new PupScreen(transparent, null, refScreens);
            ps.ScreenIndex = screenIndex;
            ps.SetRefScreen(BACKGLASS_SCREENINDEX);
            ps.HasCustomPos = true;
            ps.CustPosX = 25;
            ps.CustPosY = 25;
            ps.CustPosW = 50;
            ps.CustPosH = 50;
            ps.CalculateRealPos();
            ps.Description = "PuP screen #" + screenIndex;
            ps.Window.SetCaption( DEFAULT_SCREEN_CAPTION + screenIndex);
            ps.Active = DEFAULT_ACTIVE_MODE;
            ps.Window.Visible = true;
            this.Add(ps);
            return ps;
        }

        public static PupScreen CreateSpecial99Screen()
        {
            PupScreen screen99 = new PupScreen(true, null, null);
            screen99.ScreenIndex = SPECIAL_99_SCREENINDEX;
            return screen99;
        }

        public void RemoveOne(int screenIndex)
        {
            foreach (PupScreen ps in this)
            {
                if (ps.ScreenIndex == screenIndex)
                {
                    ps.Window.Dispose();
                    this.Remove(ps);
                    return;
                }
            }
        }
    }
}
