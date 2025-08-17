using Microsoft.AspNetCore.Mvc.Rendering;

namespace VPN_Portal.Areas.Admin.Pages
{
    public static class AdminNavPages
    {
        // Users Section
        public static string UsersIndex => "UsersIndex";
        public static string UsersCreate => "UsersCreate";
        public static string UsersManage => "UsersManage";
        
        // VPN Section
        public static string VpnIndex => "VpnIndex";
        public static string VpnServers => "VpnServers";
        
        // DNS Section
        public static string DnsIndex => "DnsIndex";
        public static string DnsPools => "DnsPools";
        public static string AddPool => "AddPool";
        
        // Navigation Classes
        public static string UsersIndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, UsersIndex);
        public static string UsersCreateNavClass(ViewContext viewContext) => PageNavClass(viewContext, UsersCreate);
        public static string UsersManageNavClass(ViewContext viewContext) => PageNavClass(viewContext, UsersManage);
        
        public static string VpnIndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, VpnIndex);
        public static string VpnServersNavClass(ViewContext viewContext) => PageNavClass(viewContext, VpnServers);
        
        public static string DnsIndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, DnsIndex);
        public static string DnsPoolsNavClass(ViewContext viewContext) => PageNavClass(viewContext, DnsPools);
        public static string DnsAddPoolNavClass(ViewContext viewContext) => PageNavClass(viewContext, AddPool);
        
        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}