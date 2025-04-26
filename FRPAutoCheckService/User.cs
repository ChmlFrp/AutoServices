using Microsoft.Win32;

namespace FRPAutoCheckService
{
    public class User
    {
        private readonly RegistryKey Key =
         Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);
        private readonly RegistryKey Key2 =
         Registry.LocalMachine.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

        //用户名  
        public  string Username
        {
            get 
            {
                return Key.GetValue("username")?.ToString()?? Key2.GetValue("username")?.ToString();
            }
            set 
            { 
                Key.SetValue("username", value);
                Key2.SetValue("username", value);
            }
        }
        
        //密码  
        public string Password
        {
            get 
            {
                return Key.GetValue("password")?.ToString()?? Key2.GetValue("password")?.ToString();
            } 
            set
            {
                Key.SetValue("password", value);
                Key2.SetValue("password", value);
            } 
        }
        
        //登录token
        public string Usertoken
        {
            get 
            {
                return Key.GetValue("usertoken")?.ToString()?? Key2.GetValue("usertoken")?.ToString(); 
            } 
            set
            {
                Key.SetValue("usertoken", value);
                Key2.SetValue("usertoken", value);
            } 
        }

        
        
    }
}
