namespace PlugSharp.Data
{
    public struct User
    {
        public string avatarID;     // AvatarID (e.g.: base01)
        public string username;     // Name of the user  
        public string language;     // ISO 639-1 representation of the language used by the user
        public bool guest;          // Is the user a guest? (this is not possible as of now)
        public int level;           // Level of the user
        public StaffRole role;            // Role of the user in this room
        public int? gRole;           // Global role of the user (0 = None; 3 = Brand Ambassador (BA); 5 = Admin)
        public string joined;       // String representation of the time the user joined plug (e.g.: 2014-07-23 22:47:00.573000)
        public int id;              // ID of the user
        public string badge;        // Badge of the user (e.g.: 80sb01)
        public string slug;         // URL conform representation of the users name (also used for the profile page)
        public int sub;             // Is the user a subscriber? (0 = false; 1 = true)
    }
}