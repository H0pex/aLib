#region • Usings

using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32;

#endregion

namespace aLib.Utils
{
    /// <summary>
    /// Работа с ресурсами операционной системы.
    /// </summary>
    public class aSystem
    {
        /// <summary>
        /// Предоставляет подробную информацию об операционной системе.
        /// </summary>
        public class OSInformation
        {
            #region ENUMS
            public enum SoftwareArchitecture
            {
                Unknown = 0,
                Bit32 = 1,
                Bit64 = 2
            }

            public enum ProcessorArchitecture
            {
                Unknown = 0,
                Bit32 = 1,
                Bit64 = 2,
                Itanium64 = 3
            }
            #endregion ENUMS

            #region DELEGATE DECLARATION
            private delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);
            #endregion DELEGATE DECLARATION

            #region BITS
            /// <summary>
            /// Determines if the current application is 32 or 64-bit.
            /// </summary>
            static public SoftwareArchitecture ProgramBits
            {
                get
                {
                    SoftwareArchitecture pbits = SoftwareArchitecture.Unknown;

                    System.Collections.IDictionary test = Environment.GetEnvironmentVariables();

                    switch (IntPtr.Size * 8)
                    {
                        case 64:
                            pbits = SoftwareArchitecture.Bit64;
                            break;

                        case 32:
                            pbits = SoftwareArchitecture.Bit32;
                            break;

                        default:
                            pbits = SoftwareArchitecture.Unknown;
                            break;
                    }

                    return pbits;
                }
            }

            static public SoftwareArchitecture OSBits
            {
                get
                {
                    SoftwareArchitecture osbits = SoftwareArchitecture.Unknown;

                    switch (IntPtr.Size * 8)
                    {
                        case 64:
                            osbits = SoftwareArchitecture.Bit64;
                            break;

                        case 32:
                            if (Is32BitProcessOn64BitProcessor())
                                osbits = SoftwareArchitecture.Bit64;
                            else
                                osbits = SoftwareArchitecture.Bit32;
                            break;

                        default:
                            osbits = SoftwareArchitecture.Unknown;
                            break;
                    }

                    return osbits;
                }
            }

            /// <summary>
            /// Determines if the current processor is 32 or 64-bit.
            /// </summary>
            static public ProcessorArchitecture ProcessorBits
            {
                get
                {
                    ProcessorArchitecture pbits = ProcessorArchitecture.Unknown;

                    try
                    {
                        SYSTEM_INFO l_System_Info = new SYSTEM_INFO();
                        GetNativeSystemInfo(ref l_System_Info);

                        switch (l_System_Info.uProcessorInfo.wProcessorArchitecture)
                        {
                            case 9: // PROCESSOR_ARCHITECTURE_AMD64
                                pbits = ProcessorArchitecture.Bit64;
                                break;
                            case 6: // PROCESSOR_ARCHITECTURE_IA64
                                pbits = ProcessorArchitecture.Itanium64;
                                break;
                            case 0: // PROCESSOR_ARCHITECTURE_INTEL
                                pbits = ProcessorArchitecture.Bit32;
                                break;
                            default: // PROCESSOR_ARCHITECTURE_UNKNOWN
                                pbits = ProcessorArchitecture.Unknown;
                                break;
                        }
                    }
                    catch
                    {
                        // Ignore        
                    }

                    return pbits;
                }
            }
            #endregion BITS

            #region EDITION
            static private string s_Edition;
            /// <summary>
            /// Gets the edition of the operating system running on this computer.
            /// </summary>
            static public string Edition
            {
                get
                {
                    if (s_Edition != null)
                        return s_Edition;  //***** RETURN *****//

                    string edition = String.Empty;

                    OperatingSystem osVersion = Environment.OSVersion;
                    OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
                    osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

                    if (GetVersionEx(ref osVersionInfo))
                    {
                        int majorVersion = osVersion.Version.Major;
                        int minorVersion = osVersion.Version.Minor;
                        byte productType = osVersionInfo.wProductType;
                        short suiteMask = osVersionInfo.wSuiteMask;

                        #region VERSION 4
                        if (majorVersion == 4)
                        {
                            if (productType == VER_NT_WORKSTATION)
                            {
                                // Windows NT 4.0 Workstation
                                edition = "Workstation";
                            }
                            else if (productType == VER_NT_SERVER)
                            {
                                if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                {
                                    // Windows NT 4.0 Server Enterprise
                                    edition = "Enterprise Server";
                                }
                                else
                                {
                                    // Windows NT 4.0 Server
                                    edition = "Standard Server";
                                }
                            }
                        }
                        #endregion VERSION 4

                        #region VERSION 5
                        else if (majorVersion == 5)
                        {
                            if (productType == VER_NT_WORKSTATION)
                            {
                                if ((suiteMask & VER_SUITE_PERSONAL) != 0)
                                {
                                    edition = "Home";
                                }
                                else
                                {
                                    if (GetSystemMetrics(86) == 0) // 86 == SM_TABLETPC
                                        edition = "Professional";
                                    else
                                        edition = "Tablet Edition";
                                }
                            }
                            else if (productType == VER_NT_SERVER)
                            {
                                if (minorVersion == 0)
                                {
                                    if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                                    {
                                        // Windows 2000 Datacenter Server
                                        edition = "Datacenter Server";
                                    }
                                    else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                    {
                                        // Windows 2000 Advanced Server
                                        edition = "Advanced Server";
                                    }
                                    else
                                    {
                                        // Windows 2000 Server
                                        edition = "Server";
                                    }
                                }
                                else
                                {
                                    if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                                    {
                                        // Windows Server 2003 Datacenter Edition
                                        edition = "Datacenter";
                                    }
                                    else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                    {
                                        // Windows Server 2003 Enterprise Edition
                                        edition = "Enterprise";
                                    }
                                    else if ((suiteMask & VER_SUITE_BLADE) != 0)
                                    {
                                        // Windows Server 2003 Web Edition
                                        edition = "Web Edition";
                                    }
                                    else
                                    {
                                        // Windows Server 2003 Standard Edition
                                        edition = "Standard";
                                    }
                                }
                            }
                        }
                        #endregion VERSION 5

                        #region VERSION 6
                        else if (majorVersion == 6)
                        {
                            int ed;
                            if (GetProductInfo(majorVersion, minorVersion,
                                osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor,
                                out ed))
                            {
                                switch (ed)
                                {
                                    case PRODUCT_BUSINESS:
                                        edition = "Business";
                                        break;
                                    case PRODUCT_BUSINESS_N:
                                        edition = "Business N";
                                        break;
                                    case PRODUCT_CLUSTER_SERVER:
                                        edition = "HPC Edition";
                                        break;
                                    case PRODUCT_CLUSTER_SERVER_V:
                                        edition = "HPC Edition without Hyper-V";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER:
                                        edition = "Datacenter Server";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER_CORE:
                                        edition = "Datacenter Server (core installation)";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER_V:
                                        edition = "Datacenter Server without Hyper-V";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER_CORE_V:
                                        edition = "Datacenter Server without Hyper-V (core installation)";
                                        break;
                                    case PRODUCT_EMBEDDED:
                                        edition = "Embedded";
                                        break;
                                    case PRODUCT_ENTERPRISE:
                                        edition = "Enterprise";
                                        break;
                                    case PRODUCT_ENTERPRISE_N:
                                        edition = "Enterprise N";
                                        break;
                                    case PRODUCT_ENTERPRISE_E:
                                        edition = "Enterprise E";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER:
                                        edition = "Enterprise Server";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_CORE:
                                        edition = "Enterprise Server (core installation)";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                                        edition = "Enterprise Server without Hyper-V (core installation)";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_IA64:
                                        edition = "Enterprise Server for Itanium-based Systems";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_V:
                                        edition = "Enterprise Server without Hyper-V";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT:
                                        edition = "Essential Business Server MGMT";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL:
                                        edition = "Essential Business Server ADDL";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC:
                                        edition = "Essential Business Server MGMTSVC";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC:
                                        edition = "Essential Business Server ADDLSVC";
                                        break;
                                    case PRODUCT_HOME_BASIC:
                                        edition = "Home Basic";
                                        break;
                                    case PRODUCT_HOME_BASIC_N:
                                        edition = "Home Basic N";
                                        break;
                                    case PRODUCT_HOME_BASIC_E:
                                        edition = "Home Basic E";
                                        break;
                                    case PRODUCT_HOME_PREMIUM:
                                        edition = "Home Premium";
                                        break;
                                    case PRODUCT_HOME_PREMIUM_N:
                                        edition = "Home Premium N";
                                        break;
                                    case PRODUCT_HOME_PREMIUM_E:
                                        edition = "Home Premium E";
                                        break;
                                    case PRODUCT_HOME_PREMIUM_SERVER:
                                        edition = "Home Premium Server";
                                        break;
                                    case PRODUCT_HYPERV:
                                        edition = "Microsoft Hyper-V Server";
                                        break;
                                    case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                                        edition = "Windows Essential Business Management Server";
                                        break;
                                    case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                                        edition = "Windows Essential Business Messaging Server";
                                        break;
                                    case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                                        edition = "Windows Essential Business Security Server";
                                        break;
                                    case PRODUCT_PROFESSIONAL:
                                        edition = "Professional";
                                        break;
                                    case PRODUCT_PROFESSIONAL_N:
                                        edition = "Professional N";
                                        break;
                                    case PRODUCT_PROFESSIONAL_E:
                                        edition = "Professional E";
                                        break;
                                    case PRODUCT_SB_SOLUTION_SERVER:
                                        edition = "SB Solution Server";
                                        break;
                                    case PRODUCT_SB_SOLUTION_SERVER_EM:
                                        edition = "SB Solution Server EM";
                                        break;
                                    case PRODUCT_SERVER_FOR_SB_SOLUTIONS:
                                        edition = "Server for SB Solutions";
                                        break;
                                    case PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM:
                                        edition = "Server for SB Solutions EM";
                                        break;
                                    case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                                        edition = "Windows Essential Server Solutions";
                                        break;
                                    case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                                        edition = "Windows Essential Server Solutions without Hyper-V";
                                        break;
                                    case PRODUCT_SERVER_FOUNDATION:
                                        edition = "Server Foundation";
                                        break;
                                    case PRODUCT_SMALLBUSINESS_SERVER:
                                        edition = "Windows Small Business Server";
                                        break;
                                    case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
                                        edition = "Windows Small Business Server Premium";
                                        break;
                                    case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE:
                                        edition = "Windows Small Business Server Premium (core installation)";
                                        break;
                                    case PRODUCT_SOLUTION_EMBEDDEDSERVER:
                                        edition = "Solution Embedded Server";
                                        break;
                                    case PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE:
                                        edition = "Solution Embedded Server (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER:
                                        edition = "Standard Server";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_CORE:
                                        edition = "Standard Server (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_SOLUTIONS:
                                        edition = "Standard Server Solutions";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE:
                                        edition = "Standard Server Solutions (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_CORE_V:
                                        edition = "Standard Server without Hyper-V (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_V:
                                        edition = "Standard Server without Hyper-V";
                                        break;
                                    case PRODUCT_STARTER:
                                        edition = "Starter";
                                        break;
                                    case PRODUCT_STARTER_N:
                                        edition = "Starter N";
                                        break;
                                    case PRODUCT_STARTER_E:
                                        edition = "Starter E";
                                        break;
                                    case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                                        edition = "Enterprise Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE:
                                        edition = "Enterprise Storage Server (core installation)";
                                        break;
                                    case PRODUCT_STORAGE_EXPRESS_SERVER:
                                        edition = "Express Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
                                        edition = "Express Storage Server (core installation)";
                                        break;
                                    case PRODUCT_STORAGE_STANDARD_SERVER:
                                        edition = "Standard Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_STANDARD_SERVER_CORE:
                                        edition = "Standard Storage Server (core installation)";
                                        break;
                                    case PRODUCT_STORAGE_WORKGROUP_SERVER:
                                        edition = "Workgroup Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
                                        edition = "Workgroup Storage Server (core installation)";
                                        break;
                                    case PRODUCT_UNDEFINED:
                                        edition = "Unknown product";
                                        break;
                                    case PRODUCT_ULTIMATE:
                                        edition = "Ultimate";
                                        break;
                                    case PRODUCT_ULTIMATE_N:
                                        edition = "Ultimate N";
                                        break;
                                    case PRODUCT_ULTIMATE_E:
                                        edition = "Ultimate E";
                                        break;
                                    case PRODUCT_WEB_SERVER:
                                        edition = "Web Server";
                                        break;
                                    case PRODUCT_WEB_SERVER_CORE:
                                        edition = "Web Server (core installation)";
                                        break;
                                }
                            }
                        }
                        #endregion VERSION 6
                    }

                    s_Edition = edition;
                    return edition;
                }
            }
            #endregion EDITION

            #region NAME
            static private string s_Name;
            /// <summary>
            /// Gets the name of the operating system running on this computer.
            /// </summary>
            static public string Name
            {
                get
                {
                    if (s_Name != null)
                        return s_Name;  //***** RETURN *****//

                    string name = "unknown";

                    OperatingSystem osVersion = Environment.OSVersion;
                    OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
                    osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

                    if (GetVersionEx(ref osVersionInfo))
                    {
                        int majorVersion = osVersion.Version.Major;
                        int minorVersion = osVersion.Version.Minor;

                        if (majorVersion == 6 && minorVersion == 2)
                        {
                            //The registry read workaround is by Scott Vickery. Thanks a lot for the help!

                            //http://msdn.microsoft.com/en-us/library/windows/desktop/ms724832(v=vs.85).aspx

                            // For applications that have been manifested for Windows 8.1 & Windows 10. Applications not manifested for 8.1 or 10 will return the Windows 8 OS version value (6.2). 
                            // By reading the registry, we'll get the exact version - meaning we can even compare against  Win 8 and Win 8.1.
                            string exactVersion = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "");
                            if (!string.IsNullOrEmpty(exactVersion))
                            {
                                string[] splitResult = exactVersion.Split('.');
                                majorVersion = Convert.ToInt32(splitResult[0]);
                                minorVersion = Convert.ToInt32(splitResult[1]);
                            }
                            if (IsWindows10())
                            {
                                majorVersion = 10;
                                minorVersion = 0;
                            }
                        }

                        switch (osVersion.Platform)
                        {
                            case PlatformID.Win32S:
                                name = "Windows 3.1";
                                break;
                            case PlatformID.WinCE:
                                name = "Windows CE";
                                break;
                            case PlatformID.Win32Windows:
                                {
                                    if (majorVersion == 4)
                                    {
                                        string csdVersion = osVersionInfo.szCSDVersion;
                                        switch (minorVersion)
                                        {
                                            case 0:
                                                if (csdVersion == "B" || csdVersion == "C")
                                                    name = "Windows 95 OSR2";
                                                else
                                                    name = "Windows 95";
                                                break;
                                            case 10:
                                                if (csdVersion == "A")
                                                    name = "Windows 98 Second Edition";
                                                else
                                                    name = "Windows 98";
                                                break;
                                            case 90:
                                                name = "Windows Me";
                                                break;
                                        }
                                    }
                                    break;
                                }
                            case PlatformID.Win32NT:
                                {
                                    byte productType = osVersionInfo.wProductType;

                                    switch (majorVersion)
                                    {
                                        case 3:
                                            name = "Windows NT 3.51";
                                            break;
                                        case 4:
                                            switch (productType)
                                            {
                                                case 1:
                                                    name = "Windows NT 4.0";
                                                    break;
                                                case 3:
                                                    name = "Windows NT 4.0 Server";
                                                    break;
                                            }
                                            break;
                                        case 5:
                                            switch (minorVersion)
                                            {
                                                case 0:
                                                    name = "Windows 2000";
                                                    break;
                                                case 1:
                                                    name = "Windows XP";
                                                    break;
                                                case 2:
                                                    name = "Windows Server 2003";
                                                    break;
                                            }
                                            break;
                                        case 6:
                                            switch (minorVersion)
                                            {
                                                case 0:
                                                    switch (productType)
                                                    {
                                                        case 1:
                                                            name = "Windows Vista";
                                                            break;
                                                        case 3:
                                                            name = "Windows Server 2008";
                                                            break;
                                                    }
                                                    break;

                                                case 1:
                                                    switch (productType)
                                                    {
                                                        case 1:
                                                            name = "Windows 7";
                                                            break;
                                                        case 3:
                                                            name = "Windows Server 2008 R2";
                                                            break;
                                                    }
                                                    break;
                                                case 2:
                                                    switch (productType)
                                                    {
                                                        case 1:
                                                            name = "Windows 8";
                                                            break;
                                                        case 3:
                                                            name = "Windows Server 2012";
                                                            break;
                                                    }
                                                    break;
                                                case 3:
                                                    switch (productType)
                                                    {
                                                        case 1:
                                                            name = "Windows 8.1";
                                                            break;
                                                        case 3:
                                                            name = "Windows Server 2012 R2";
                                                            break;
                                                    }
                                                    break;
                                            }
                                            break;
                                        case 10:
                                            switch (minorVersion)
                                            {
                                                case 0:
                                                    switch (productType)
                                                    {
                                                        case 1:
                                                            name = "Windows 10";
                                                            break;
                                                        case 3:
                                                            name = "Windows Server 2016";
                                                            break;
                                                    }
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                }
                        }
                    }

                    s_Name = name;
                    return name;
                }
            }
            #endregion NAME

            #region PINVOKE

            #region GET
            #region PRODUCT INFO
            [DllImport("Kernel32.dll")]
            internal static extern bool GetProductInfo(
                int osMajorVersion,
                int osMinorVersion,
                int spMajorVersion,
                int spMinorVersion,
                out int edition);
            #endregion PRODUCT INFO

            #region VERSION
            [DllImport("kernel32.dll")]
            private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);
            #endregion VERSION

            #region SYSTEMMETRICS
            [DllImport("user32")]
            public static extern int GetSystemMetrics(int nIndex);
            #endregion SYSTEMMETRICS

            #region SYSTEMINFO
            [DllImport("kernel32.dll")]
            public static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

            [DllImport("kernel32.dll")]
            public static extern void GetNativeSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);
            #endregion SYSTEMINFO

            #endregion GET

            #region OSVERSIONINFOEX
            [StructLayout(LayoutKind.Sequential)]
            private struct OSVERSIONINFOEX
            {
                public int dwOSVersionInfoSize;
                public int dwMajorVersion;
                public int dwMinorVersion;
                public int dwBuildNumber;
                public int dwPlatformId;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string szCSDVersion;
                public short wServicePackMajor;
                public short wServicePackMinor;
                public short wSuiteMask;
                public byte wProductType;
                public byte wReserved;
            }
            #endregion OSVERSIONINFOEX

            #region SYSTEM_INFO
            [StructLayout(LayoutKind.Sequential)]
            public struct SYSTEM_INFO
            {
                internal _PROCESSOR_INFO_UNION uProcessorInfo;
                public uint dwPageSize;
                public IntPtr lpMinimumApplicationAddress;
                public IntPtr lpMaximumApplicationAddress;
                public IntPtr dwActiveProcessorMask;
                public uint dwNumberOfProcessors;
                public uint dwProcessorType;
                public uint dwAllocationGranularity;
                public ushort dwProcessorLevel;
                public ushort dwProcessorRevision;
            }
            #endregion SYSTEM_INFO

            #region _PROCESSOR_INFO_UNION
            [StructLayout(LayoutKind.Explicit)]
            public struct _PROCESSOR_INFO_UNION
            {
                [FieldOffset(0)]
                internal uint dwOemId;
                [FieldOffset(0)]
                internal ushort wProcessorArchitecture;
                [FieldOffset(2)]
                internal ushort wReserved;
            }
            #endregion _PROCESSOR_INFO_UNION

            #region 64 BIT OS DETECTION
            [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            public extern static IntPtr LoadLibrary(string libraryName);

            [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            public extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);
            #endregion 64 BIT OS DETECTION

            #region PRODUCT
            private const int PRODUCT_UNDEFINED = 0x00000000;
            private const int PRODUCT_ULTIMATE = 0x00000001;
            private const int PRODUCT_HOME_BASIC = 0x00000002;
            private const int PRODUCT_HOME_PREMIUM = 0x00000003;
            private const int PRODUCT_ENTERPRISE = 0x00000004;
            private const int PRODUCT_HOME_BASIC_N = 0x00000005;
            private const int PRODUCT_BUSINESS = 0x00000006;
            private const int PRODUCT_STANDARD_SERVER = 0x00000007;
            private const int PRODUCT_DATACENTER_SERVER = 0x00000008;
            private const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
            private const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
            private const int PRODUCT_STARTER = 0x0000000B;
            private const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
            private const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
            private const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
            private const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
            private const int PRODUCT_BUSINESS_N = 0x00000010;
            private const int PRODUCT_WEB_SERVER = 0x00000011;
            private const int PRODUCT_CLUSTER_SERVER = 0x00000012;
            private const int PRODUCT_HOME_SERVER = 0x00000013;
            private const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
            private const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
            private const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
            private const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
            private const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
            private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
            private const int PRODUCT_HOME_PREMIUM_N = 0x0000001A;
            private const int PRODUCT_ENTERPRISE_N = 0x0000001B;
            private const int PRODUCT_ULTIMATE_N = 0x0000001C;
            private const int PRODUCT_WEB_SERVER_CORE = 0x0000001D;
            private const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
            private const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
            private const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
            private const int PRODUCT_SERVER_FOUNDATION = 0x00000021;
            private const int PRODUCT_HOME_PREMIUM_SERVER = 0x00000022;
            private const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
            private const int PRODUCT_STANDARD_SERVER_V = 0x00000024;
            private const int PRODUCT_DATACENTER_SERVER_V = 0x00000025;
            private const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
            private const int PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;
            private const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
            private const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
            private const int PRODUCT_HYPERV = 0x0000002A;
            private const int PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B;
            private const int PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C;
            private const int PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D;
            private const int PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E;
            private const int PRODUCT_STARTER_N = 0x0000002F;
            private const int PRODUCT_PROFESSIONAL = 0x00000030;
            private const int PRODUCT_PROFESSIONAL_N = 0x00000031;
            private const int PRODUCT_SB_SOLUTION_SERVER = 0x00000032;
            private const int PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033;
            private const int PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034;
            private const int PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035;
            private const int PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036;
            private const int PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037;
            private const int PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038;
            private const int PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE = 0x00000039;
            //private const int ???? = 0x0000003A;
            private const int PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B;
            private const int PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C;
            private const int PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D;
            private const int PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E;
            private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 0x0000003F;
            private const int PRODUCT_CLUSTER_SERVER_V = 0x00000040;
            private const int PRODUCT_EMBEDDED = 0x00000041;
            private const int PRODUCT_STARTER_E = 0x00000042;
            private const int PRODUCT_HOME_BASIC_E = 0x00000043;
            private const int PRODUCT_HOME_PREMIUM_E = 0x00000044;
            private const int PRODUCT_PROFESSIONAL_E = 0x00000045;
            private const int PRODUCT_ENTERPRISE_E = 0x00000046;
            private const int PRODUCT_ULTIMATE_E = 0x00000047;
            //private const int PRODUCT_UNLICENSED = 0xABCDABCD;
            #endregion PRODUCT

            #region VERSIONS
            private const int VER_NT_WORKSTATION = 1;
            private const int VER_NT_DOMAIN_CONTROLLER = 2;
            private const int VER_NT_SERVER = 3;
            private const int VER_SUITE_SMALLBUSINESS = 1;
            private const int VER_SUITE_ENTERPRISE = 2;
            private const int VER_SUITE_TERMINAL = 16;
            private const int VER_SUITE_DATACENTER = 128;
            private const int VER_SUITE_SINGLEUSERTS = 256;
            private const int VER_SUITE_PERSONAL = 512;
            private const int VER_SUITE_BLADE = 1024;
            #endregion VERSIONS

            #endregion PINVOKE

            #region SERVICE PACK
            /// <summary>
            /// Gets the service pack information of the operating system running on this computer.
            /// </summary>
            static public string ServicePack
            {
                get
                {
                    string servicePack = String.Empty;
                    OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();

                    osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

                    if (GetVersionEx(ref osVersionInfo))
                    {
                        servicePack = osVersionInfo.szCSDVersion;
                    }

                    return servicePack;
                }
            }
            #endregion SERVICE PACK

            #region VERSION
            #region BUILD
            /// <summary>
            /// Gets the build version number of the operating system running on this computer.
            /// </summary>
            static public int BuildVersion
            {
                get
                {
                    return int.Parse(RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", "0"));
                }
            }
            #endregion BUILD

            #region FULL
            #region STRING
            /// <summary>
            /// Gets the full version string of the operating system running on this computer.
            /// </summary>
            static public string VersionString
            {
                get
                {
                    return Version.ToString();
                }
            }
            #endregion STRING

            #region VERSION
            /// <summary>
            /// Gets the full version of the operating system running on this computer.
            /// </summary>
            static public Version Version
            {
                get
                {
                    return new Version(MajorVersion, MinorVersion, BuildVersion, RevisionVersion);
                }
            }
            #endregion VERSION
            #endregion FULL

            #region MAJOR
            /// <summary>
            /// Gets the major version number of the operating system running on this computer.
            /// </summary>
            static public int MajorVersion
            {
                get
                {
                    if (IsWindows10())
                    {
                        return 10;
                    }
                    string exactVersion = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "");
                    if (!string.IsNullOrEmpty(exactVersion))
                    {
                        string[] splitVersion = exactVersion.Split('.');
                        return int.Parse(splitVersion[0]);
                    }
                    return Environment.OSVersion.Version.Major;
                }
            }
            #endregion MAJOR

            #region MINOR
            /// <summary>
            /// Gets the minor version number of the operating system running on this computer.
            /// </summary>
            static public int MinorVersion
            {
                get
                {
                    if (IsWindows10())
                    {
                        return 0;
                    }
                    string exactVersion = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "");
                    if (!string.IsNullOrEmpty(exactVersion))
                    {
                        string[] splitVersion = exactVersion.Split('.');
                        return int.Parse(splitVersion[1]);
                    }
                    return Environment.OSVersion.Version.Minor;
                }
            }
            #endregion MINOR

            #region REVISION
            /// <summary>
            /// Gets the revision version number of the operating system running on this computer.
            /// </summary>
            static public int RevisionVersion
            {
                get
                {
                    if (IsWindows10())
                    {
                        return 0;
                    }
                    return Environment.OSVersion.Version.Revision;
                }
            }
            #endregion REVISION
            #endregion VERSION

            #region 64 BIT OS DETECTION
            private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
            {
                IntPtr handle = LoadLibrary("kernel32");

                if (handle != IntPtr.Zero)
                {
                    IntPtr fnPtr = GetProcAddress(handle, "IsWow64Process");

                    if (fnPtr != IntPtr.Zero)
                    {
                        return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer((IntPtr)fnPtr, typeof(IsWow64ProcessDelegate));
                    }
                }

                return null;
            }

            private static bool Is32BitProcessOn64BitProcessor()
            {
                IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

                if (fnDelegate == null)
                {
                    return false;
                }

                bool isWow64;
                bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out isWow64);

                if (retVal == false)
                {
                    return false;
                }

                return isWow64;
            }
            #endregion 64 BIT OS DETECTION

            #region Windows 10 Detection

            private static bool IsWindows10()
            {
                string productName = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "");
                if (productName.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }

            #endregion

            #region Registry Methods

            private static string RegistryRead(string RegistryPath, string Field, string DefaultValue)
            {
                string rtn = "";
                string backSlash = "";
                string newRegistryPath = "";

                try
                {
                    RegistryKey OurKey = null;
                    string[] split_result = RegistryPath.Split('\\');

                    if (split_result.Length > 0)
                    {
                        split_result[0] = split_result[0].ToUpper();        // Make the first entry uppercase...

                        if (split_result[0] == "HKEY_CLASSES_ROOT") OurKey = Registry.ClassesRoot;
                        else if (split_result[0] == "HKEY_CURRENT_USER") OurKey = Registry.CurrentUser;
                        else if (split_result[0] == "HKEY_LOCAL_MACHINE") OurKey = Registry.LocalMachine;
                        else if (split_result[0] == "HKEY_USERS") OurKey = Registry.Users;
                        else if (split_result[0] == "HKEY_CURRENT_CONFIG") OurKey = Registry.CurrentConfig;

                        if (OurKey != null)
                        {
                            for (int i = 1; i < split_result.Length; i++)
                            {
                                newRegistryPath += backSlash + split_result[i];
                                backSlash = "\\";
                            }

                            if (newRegistryPath != "")
                            {
                                //rtn = (string)Registry.GetValue(RegistryPath, "CurrentVersion", DefaultValue);

                                OurKey = OurKey.OpenSubKey(newRegistryPath);
                                rtn = (string)OurKey.GetValue(Field, DefaultValue);
                                OurKey.Close();
                            }
                        }
                    }
                }
                catch { }

                return rtn;
            }

            #endregion Registry Methods
        }

        /// <summary>
        /// Системные сообщения приложения.
        /// </summary>
        public class Messages
        {
            /// <summary>
            /// Допустимые типы сообщений.
            /// </summary>
            public enum MessageTypes
            {
                /// <summary>
                /// Тип исключения приложения.
                /// </summary>
                Exceptions,

                /// <summary>
                /// Информационный тип сообщения.
                /// </summary>
                Inforamtion,

                /// <summary>
                /// Сообщение вопросительного типа.
                /// </summary>
                Question,

                /// <summary>
                /// Тип сообщения повышенной важности.
                /// </summary>
                Warning
            }

            /// <summary>
            /// Показ сообщения с отпределнным типом <paramref name="MessageType"/>.
            /// </summary>
            /// <param name="MessageText">Текст сообщения.</param>
            /// <param name="MessageType">Тип сообщения.</param>
            public static DialogResult Show(string MessageText, MessageTypes MessageType = MessageTypes.Inforamtion)
            {
                MessageBoxButtons MessageBoxButtons = default;
                MessageBoxIcon MessageBoxIcon = default;

                switch (MessageType)
                {
                    case MessageTypes.Exceptions:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Error;
                        break;
                    case MessageTypes.Inforamtion:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Information;
                        break;
                    case MessageTypes.Question:
                        MessageBoxButtons = MessageBoxButtons.YesNo;
                        MessageBoxIcon = MessageBoxIcon.Question;
                        break;
                    case MessageTypes.Warning:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Exclamation;
                        break;
                    default:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Information;
                        break;
                }

                return MessageBox.Show(MessageText, Application.ProductName, MessageBoxButtons, MessageBoxIcon);
            }
        }

        /// <summary>
        /// Перечисление типов сообщения.
        /// </summary>
        public enum NotifyType
        {
            Success,
            Error,
            Warning,
            Information
        };
        public static int NotifyCount = 0;

        /// <summary>
        /// Выводит сообщение пользователя как уведомление в правом верхнем углу.
        /// </summary>
        /// <param name="Data">Текст сообщения.</param>
        /// <param name="NotifyType">Тип сообщения; по умолчанию Information.</param>
        /// <param name="TimeOut">Тип сообщения; по умолчанию 3000 (3 сек.).</param>
        public static async void NotifyBox(string Data, NotifyType NotifyType = NotifyType.Information, int TimeOut = 3000)
        {
            #region Message

            Form Message = new Form()
            {
                Opacity = 1, // 0 для анимации
                BackColor = Color.FromArgb(17, 17, 17),
                Size = new Size(320, 100), // (1, 100) для анимации
                StartPosition = FormStartPosition.Manual,
                DesktopLocation = new Point(
                        Convert.ToInt32(GetUserSreen()[0]) - 330,
                        NotifyCount < 1 ? 10 : NotifyCount * 120),
                FormBorderStyle = FormBorderStyle.None,
                ControlBox = false,
                ShowInTaskbar = false,
                TopMost = true
            };

            #endregion

            #region Line

            var sideColor = Color.Silver;
            switch (NotifyType)
            {
                case NotifyType.Success:
                    sideColor = Color.Green;
                    break;
                case NotifyType.Error:
                    sideColor = Color.Red;
                    break;
                case NotifyType.Warning:
                    sideColor = Color.Yellow;
                    break;
                case NotifyType.Information:
                    sideColor = Color.Silver;
                    break;
                default:
                    sideColor = Color.Silver;
                    break;
            }

            PictureBox Line = new PictureBox()
            {
                Parent = Message,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Location = new Point(0, 0),
                Size = new Size(2, 100),
                BackColor = sideColor
            };

            #endregion

            #region Settings

            //PictureBox Settings = new PictureBox()
            //{
            //    Parent = Message,
            //    BackColor = Color.Transparent,
            //    Size = new Size(10, 10),
            //    Location = new Point(292, 4),
            //    SizeMode = PictureBoxSizeMode.StretchImage,
            //    Image = Properties.Resources.NSettings
            //};

            #endregion

            #region Close

            //PictureBox Exit = new PictureBox()
            //{
            //    Parent = Message,
            //    BackColor = Color.Transparent,
            //    Size = new Size(10, 10),
            //    Location = new Point(306, 4),
            //    SizeMode = PictureBoxSizeMode.StretchImage,
            //    Image = Properties.Resources.NExit
            //};
            //Exit.MouseClick += (object O, MouseEventArgs E) => { Message.Dispose(); NotifyCount--; };

            #endregion

            #region Caption

            Label mCaption = new Label()
            {
                Parent = Message,
                AutoSize = true,
                Location = new Point(7, 4),
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Point),
                Text = (NotifyType == NotifyType.Information
                        ? "Информация"
                        : (NotifyType == NotifyType.Error
                            ? "Ошибка"
                            : "Внимание"))
            };

            #endregion

            #region Data

            Label mData = new Label()
            {
                Parent = Message,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Size = new Size(310, 70),
                Location = new Point(7, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                Text = Data
            };

            #endregion

            NotifyCount++;
            Message.Show();
            await Task.Run(() => Thread.Sleep(TimeOut));
            NotifyCount--;
            Message.Dispose();

            // Анимация
            // aAnimations.AlphaBlend.AlphaBlandMessage(Message, 10, Time);
        }

        public static async Task<bool> ExistsProcessAsync(string processName) => await Task.Run(() => Process.GetProcessesByName(processName).Any());
        public static bool ExistsProcess(string processName) => Process.GetProcessesByName(processName).Any();
        
        public static async Task KillProcessAsync(string processName)
        {
            await Task.Run(() =>
            {
                List<Process> processes = new List<Process>();
                processes.AddRange(Process.GetProcessesByName(processName));
                processes.ForEach(x => x.Kill());
            });
        }

        public static void KillProcess(string processName)
        {
            List<Process> processes = new List<Process>();
            processes.AddRange(Process.GetProcessesByName(processName));
            processes.ForEach(x => x.Kill());
        }

        /* Не дописано диалоговое окно */

        //public static DialogResult MessageBox(string Text, MessageType MessageType)
        //{
        //    #region Message

        //    Form Message = new Form()
        //    {
        //        BackColor = Color.FromArgb(17, 17, 17),
        //        Size = new Size(350, 150),
        //        StartPosition = FormStartPosition.CenterScreen,
        //        DesktopLocation = new Point(
        //                (GetUserSreen()[0] - 350) / 2,
        //                (GetUserSreen()[1] - 150) / 2),
        //        FormBorderStyle = FormBorderStyle.None,
        //        ControlBox = false,
        //        ShowInTaskbar = false,
        //        TopMost = true
        //    };

        //    #endregion

        //    #region Caption

        //    Label Caption = new Label()
        //    {
        //        Parent = Message,
        //        AutoSize = true,
        //        Location = new Point(7, 4),
        //        ForeColor = Color.Silver,
        //        Font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Point),
        //        Text = (MessageType == MessageType.Information
        //                ? "Информация"
        //                : (MessageType == MessageType.Error
        //                    ? "Ошибка"
        //                    : "Внимание"))
        //    };

        //    #endregion

        //    #region Data

        //    Label Data = new Label()
        //    {
        //        Parent = Message,
        //        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
        //        Size = new Size(310, 70),
        //        Location = new Point(7, 25),
        //        ForeColor = Color.White,
        //        Font = new Font("Segoe UI", 8, FontStyle.Regular),
        //        Text = Text
        //    };

        //    #endregion

        //    #region Buttons



        //    #endregion
        //}

        /// <summary>
        /// Скрытый интерпретатор командной строки.
        /// </summary>
        public class HCmd
        {
            /// <summary>
            /// Проверяет доступность интернет-соединения.
            /// </summary>
            public class IsSetWebConnection
            {
                /// <summary>
                /// Проверяет доступность серверов Google.
                /// </summary>
                public static bool Google()
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.google.ru/");
                        request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                        request.Timeout = 10000;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream ReceiveStream1 = response.GetResponseStream();
                        StreamReader sr = new StreamReader(ReceiveStream1, true);
                        string responseFromServer = sr.ReadToEnd();

                        response.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                /// <summary>
                /// Проверяет доступность сервера, указанного пользователем.
                /// </summary>
                /// <param name="Url">Проверяемый сервер.</param>
                public static bool Custom(string Url)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
                        request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                        request.Timeout = 10000;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream ReceiveStream1 = response.GetResponseStream();
                        StreamReader sr = new StreamReader(ReceiveStream1, true);
                        string responseFromServer = sr.ReadToEnd();
                        response.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            /// Выполняет введенную команду пользователя.
            /// </summary>
            /// <param name="Command">Исполняемая команда.</param>
            public static void Custom(string Command)
            {
                ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C " + Command);

                psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                psiOpt.RedirectStandardOutput = true;
                psiOpt.UseShellExecute = false;
                psiOpt.CreateNoWindow = true;

                Process procCommand = Process.Start(psiOpt);
                StreamReader srIncoming = procCommand.StandardOutput;
                procCommand.WaitForExit();
            }

            /// <summary>
            /// Уничтожает заданный пользователем процесс, если тот доступен.
            /// </summary>
            /// <param name="ProcessName">Уничтожаемый процесс.</param>
            public static void KillProcess(string ProcessName)
            {
                Custom("taskkill /f /im " + ProcessName);
            }

            /// <summary>
            /// Выполняет поиск заданного пользователем процесса.
            /// </summary>
            /// <param name="Name">Искомый процесс.</param>
            public static async Task<bool> ExistProcess(string Name)
            {
                bool cont = false;
                await Task.Run(() =>
                {
                    foreach (Process clsProcess in Process.GetProcesses())
                        if (clsProcess.ProcessName.StartsWith(Name))
                        {
                            cont = true;
                            break;
                        }                    
                });
                return cont;
            }
        }

        /// <summary>
        /// Импорт библиотеки.
        /// </summary>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        /// <summary>
        /// Возвращает объем доступной оперативной памяти.
        /// </summary>
        /// <param name="TotalMemoryInKilobytes">Тотальный объем оперативной памяти.</param>
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        /// <summary>
        /// Возвращает запрещенное приложение (совместно с MARK46).
        /// </summary>
        public static string CheckDBG()
        {
            string X = "0x01";
            string[] PS = GetProccessesCaption().Split('~');
            string[] xPS =
                    new string[]
                    {
                    "[F?Ш???]",             "[BlackBox]",           "Sandbox:D",
                    "IDA: Quick start",     "PE Explorer",          "AssemblyExplorer",
                    "Microsoft Debugger",   "Syser Kernel",         "syser",
                    "PEID",                 "PEiD",                 "peid",
                    "SNIFFER",              "Sniffer",              "sniffer",
                    "DUMP",                 "Dump",                 "dump",
                    "DUMPER",               "Dumper",               "dumper",
                    "PE TOOL",              "PE Tool",              "pe tool",
                    "PETOOLS",              "PETools",              "petools",
                    "DUPAR",                "Dupar",                "dupar",
                    "DUPER",                "Duper",                "duper",
                    "ENGINE",               "Engine",               "engine",
                    "OLLY",                 "Olly",                 "olly",
                    "PACKAGER",             "Packager",             "packager",
                    "PACOTES",              "Pacotes",              "pacotes",
                    "SUSPEND",              "Suspend",              "suspend",
                    "WILDPROXY",            "Wildproxy",            "wildproxy",
                    "XELERATOR",            "Xelerator",            "xeleretor",
                    "PACKET",               "Packet",               "packet",
                    "SANDBOX",              "Sandbox",              "sandbox",
                    "SANDBOXED",            "Sandboxed",            "sandboxed",
                    "DEFALTBOX",            "DefaltBox",            "defaltbox",
                    "HXD",                  "HxD",                  "hxd",
                    "BVKHEX",               "Bvkhex",               "bvkhex",
                    "EMULATOR:",            "Emulator:",            "emulator:",
                    "AUTOCLIKER",           "AutoClicker",          "autoclicker",
                    "DEBUG",                "Debug",                "debug",
                    "DEBUGER",              "Debugger",             "debugger",
                    "SYSERAPP",             "SyserApp",             "syserapp",
                    "OLLYDBG",              "OllyDgb",              "ollydbg",
                    "SPY",                  "Spy",                  "spy",
                    "ilspy",                "ILSpy",                "ILSPY",
                    "DNSPY",                "dnSpy",                "dnspy",
                    "IMMUNITY",             "Immunity",             "immunity",
                    "YDBG",                 "YDbg",                 "tdbg",
                    "SOFTICE",              "SoftICE",              "softice",
                    "GDB",                  "Gdb",                  "gdb",
                    "IDA",                  "Ida",                  "ida",
                    "HEX-RAYS",             "Hex-Rays",             "hex-rays",
                    "W32DASM",              "W32Dasm",              "w32dasm",
                    "DEDE",                 "DeDe",                 "dede",
                    "PEID",                 "PEiD",                 "peid"
                    };

            foreach (var ProcU in PS)
            {
                foreach (var ProcX in xPS)
                {
                    if (ProcU.IndexOf(ProcX) != -1)
                    {
                        X = ProcX + "|" + GetProccesPath(ProcX);
                    }
                }
            }
            return X;
        }

        /// <summary>
        /// Список установленных версий .NET Framework.
        /// </summary>
        public static List<string> DotNetFrameworkVersions
        {
            get => new DotNetFramework().Versions;
        }

        private class DotNetFramework
        {
            /// <summary>
            /// Список установленных версий .NET Framework.
            /// </summary>
            internal List<string> Versions = new List<string>();

            public DotNetFramework()
            {
                // Показываем все установленные версии.
                Get1To45VersionFromRegistry();
                Get45PlusFromRegistry();
            }

            // Запись версии.
            private void WriteVersion(string version, string spLevel = "")
            {
                version = version.Trim();
                if (string.IsNullOrEmpty(version))
                    return;

                string spLevelString = "";
                if (!string.IsNullOrEmpty(spLevel))
                    spLevelString = " Service Pack " + spLevel;

                Versions.Add($"{version}{ spLevelString}");
                //Console.WriteLine($"{version}{spLevelString}");
            }

            private void Get1To45VersionFromRegistry()
            {
                // Открываем ключ реестра для записи .NET Framework.
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                {
                    foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                    {
                        // Пропускаем информацию о версии 4.5.
                        if (versionKeyName == "v4")                        
                            continue;                        

                        if (versionKeyName.StartsWith("v"))
                        {
                            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);

                            // Получаем значение .NET Framework версии.
                            string name = (string)versionKey.GetValue("Version", "");
                            
                            // Получаем номер сервисного пакета (SP).
                            string sp = versionKey.GetValue("SP", "").ToString();

                            // Получаем флаг установки или пустую строку, если ее нет.
                            string install = versionKey.GetValue("Install", "").ToString();

                            // Нет информации об установке, это должно быть в дочернем подразделе.
                            if (string.IsNullOrEmpty(install)) 
                                WriteVersion(name);
                            else
                            {
                                if (!(string.IsNullOrEmpty(sp)) && install == "1")                                
                                    WriteVersion(name, sp);                                
                            }
                            if (!string.IsNullOrEmpty(name))                            
                                continue;                            

                            foreach (string subKeyName in versionKey.GetSubKeyNames())
                            {
                                RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                                name = (string)subKey.GetValue("Version", "");
                                if (!string.IsNullOrEmpty(name))
                                    sp = subKey.GetValue("SP", "").ToString();

                                install = subKey.GetValue("Install", "").ToString();

                                // Нет информации об установке.
                                if (string.IsNullOrEmpty(install))
                                    WriteVersion(name);
                                else
                                {
                                    if (!(string.IsNullOrEmpty(sp)) && install == "1")
                                        WriteVersion(name, sp);
                                    else if (install == "1")
                                        WriteVersion(name);

                                }
                            }
                        }
                    }
                }
            }

            private void Get45PlusFromRegistry()
            {
                const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(subkey))
                {
                    if (ndpKey == null)
                        return;

                    // Сначала проверяем, указана ли конкретная версия.
                    if (ndpKey.GetValue("Version") != null)
                    {
                        WriteVersion(ndpKey.GetValue("Version").ToString());
                    }
                    else
                    {
                        if (ndpKey != null && ndpKey.GetValue("Release") != null)
                        {
                            WriteVersion(
                                CheckFor45PlusVersion(
                                        (int)ndpKey.GetValue("Release")
                                    )
                            );
                        }
                    }
                }

                // Проверка версии с использованием >= включает совместимость.
                string CheckFor45PlusVersion(int releaseKey)
                {
                    if (releaseKey >= 528040)
                        return "4.8";
                    if (releaseKey >= 461808)
                        return "4.7.2";
                    if (releaseKey >= 461308)
                        return "4.7.1";
                    if (releaseKey >= 460798)
                        return "4.7";
                    if (releaseKey >= 394802)
                        return "4.6.2";
                    if (releaseKey >= 394254)
                        return "4.6.1";
                    if (releaseKey >= 393295)
                        return "4.6";
                    if (releaseKey >= 379893)
                        return "4.5.2";
                    if (releaseKey >= 378675)
                        return "4.5.1";
                    if (releaseKey >= 378389)
                        return "4.5";
                    // Этот код никогда не должен выполняться. Ключ не нулевого выпуска должен означать
                    // что 4,5 или более поздний уже установлен.
                    return "";
                }
            }
        }

        /// <summary>
        /// Возвращает битность ОС Windows (x32/x64).
        /// </summary>
        public static string GetWinBit() => Environment.Is64BitOperatingSystem ? "64" : "32";

        /// <summary>
        /// Возвращает название Windows.
        /// </summary>
        /// <param name="OperatingSystemVersion">Объект версии операционной системы – Environment.OSVersion.Version.</param>
        public static string GetWinVersion(Version OperatingSystemVersion)
        {
            switch ((OperatingSystemVersion.Major + "." + OperatingSystemVersion.Minor).ToString())
            {
                case "5.0": return "Windows 2000";
                case "5.1": return "Windows XP";
                case "5.2": return "Windows XP (R2)";
                case "6.0": return "Windows Vista";
                case "6.1": return "Windows 7";
                case "6.2": return "Windows 8";
                case "6.3": return "Windows 8.1";
                case "10.0": return "Windows 10";
                default: return "Unknow";
            }
        }

        /// <summary>
        /// Возвращает массив имен (через ~) всех запущенных процессов.
        /// </summary>
        public static string GetProccesses()
        {
            string response = null;
            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes) response += mo["Name"].ToString() + "~";
            return response;

        }

        /// <summary>
        /// Возвращает массив описаний (через ~) всех запущенных процессов.
        /// </summary>
        public static string GetProccessesCaption()
        {
            string response = null;
            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes) response += mo["Caption"].ToString() + "~";
            return response;

        }

        /// <summary>
        /// Возвращает имя процесса по ID.
        /// </summary>
        /// <param name="ID">ID процесса.</param>
        public static string GetProccesName(int ID)
        {
            return Process.GetProcessById(ID).ProcessName;
        }

        /// <summary>
        /// Возвращает ID процесса по имени.
        /// </summary>
        /// <param name="Name">Имя процесса.</param>
        public static string GetProccessID(string Name)
        {
            int response = 00;
            Name = Name.Contains(".") ? Name.Split('.')[1] == "exe" ? Name : Name + "exe" : Name + ".exe";

            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes)
            {
                if (mo["Name"].ToString() == Name)
                    response = Convert.ToInt32(mo["ProcessId"]);
            }
            return response.ToString();
        }

        /// <summary>
        /// Возвращает путь до файла процесса по ID.
        /// </summary>
        /// <param name="ID">ID процесса.</param>
        public static string GetProccesPath(int ID)
        {
            string response = null;

            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes)
            {
                if (Convert.ToInt32(mo["ProcessId"]) == ID)
                    response = mo["ExecutablePath"].ToString();
            }
            return response;

        }

        /// <summary>
        /// Возвращает путь до файла процесса по имени.
        /// </summary>
        /// <param name="Name">Имя процесса.</param>
        public static string GetProccesPath(string Name)
        {
            if (Name != "0x01")
            {
                string response = string.Empty;
                //Name = Name.Contains(".") ? Name.Split('.')[1] == "exe" ? Name : Name + "exe" : Name + ".exe";
                Name = Name.Contains(".exe") ? Name.Replace(".exe", "") : Name;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT *  FROM Win32_Process");
                foreach (ManagementObject queryObj in searcher.Get())
                    if (queryObj["Name"].ToString().Contains(Name)) response = queryObj["ExecutablePath"].ToString();
                return response;
            }
            else return "";
        }

        /// <summary>
        /// Возвращает имя компьютера.
        /// </summary>
        public static string GetNamePC() => Environment.MachineName;

        /// <summary>
        /// Возвращает имя пользователя.
        /// </summary>
        public static string GetNameUser() => Environment.UserName;        

        /// <summary>
        /// Возвращает размер экрана пользователя.
        /// </summary>
        public static int [] GetUserSreen()
        {
            string Uscreen = string.Empty;
            foreach (var screen in Screen.AllScreens)
                Uscreen = screen.WorkingArea.ToString();
            return new int[] { int.Parse(Uscreen.Split(',')[2].Split('=')[1]), int.Parse(Uscreen.Split(',')[3].Split('=')[1].Replace("}", "")) };
        }

        /// <summary>
        /// Возвращает тотальный объем оперативной памяти.
        /// </summary>
        public static string GetUserRam()
        {
            long memKb;
            GetPhysicallyInstalledSystemMemory(out memKb);
            return (memKb / 1024 / 1024).ToString();

        }

        /// <summary>
        /// Возвращает количество ядер процессора пользователя.
        /// </summary>
        public static string GetUserCoreCount()
        {
            return Environment.ProcessorCount.ToString();
        }

        /// <summary>
        /// Перечисление языков единицы измерения.
        /// </summary>
        public enum Language { Ru, En };

        /// <summary>
        /// Возвращает список подключенных дисков и их объем в ГБ.
        /// </summary>
        /// <param name="Language">Язык единицы измерения.</param>
        public static string[,] GetUserDrives(Language Language)
        {
            bool LNG = Language == Language.Ru ? true : false;
            string[,] drives = new string[Environment.GetLogicalDrives().Length, 2];
            for (int i = 0; i < Environment.GetLogicalDrives().Length; i++)
            {
                string drive = Environment.GetLogicalDrives()[i];
                try
                {
                    drives[i, 0] =
                    (LNG ? "Диск " : "Drive ") + drive.Replace(":\\", "") + (LNG ? "(ГБ)" : "(GB)");
                    drives[i, 1] =
                    (Math.Round((double)(new DriveInfo(drive).TotalSize - new DriveInfo(drive).AvailableFreeSpace) / 1000000000, 0)).ToString()
                    + "/" +
                    (Math.Round((double)new DriveInfo(drive).TotalSize / 1000000000, 0)).ToString();
                }
                catch { }
            }
            return drives;
        }
    }
}
