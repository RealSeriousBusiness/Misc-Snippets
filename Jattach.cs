using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//super messy, please just ignore this file
namespace Snippets
{
    /// <summary>
    /// Attaches a java agent to a running java program.
    /// </summary>
    class Native
    {
        public struct JavaVMInitArgs
        {
            public int version;
            public int nOptions;
            public IntPtr options;
            public int ignoreUnrecognized;
        }

        public static void Attach(int pid, string path, string arguments = "")
        {
            IntPtr vm = IntPtr.Zero, env = IntPtr.Zero;

            JavaVMInitArgs args = new JavaVMInitArgs();
            args.version = 0x00010008;
            args.nOptions = 0;

            int result = JNI_CreateJavaVM(ref vm, ref env, ref args);

            Console.WriteLine("result " + result);


            IntPtr real = Marshal.ReadIntPtr(env);
            IntPtr function = Marshal.ReadIntPtr(real, 668);
            IntPtr functionL = Marshal.ReadIntPtr(real, 656);

            IntPtr realVmStruct = Marshal.ReadIntPtr(vm);
            

            StringDelegate newStringUTF = Marshal.GetDelegateForFunctionPointer<StringDelegate>(function);
            GetStringLength stringLen = Marshal.GetDelegateForFunctionPointer<GetStringLength>(functionL);
            ExceptionCheck checkException = Marshal.GetDelegateForFunctionPointer<ExceptionCheck>(Marshal.ReadIntPtr(real, 912));

            ExceptionDescribe printException = Marshal.GetDelegateForFunctionPointer<ExceptionDescribe>(Marshal.ReadIntPtr(real, 64));

            NewObjectArray newObjectArray = Marshal.GetDelegateForFunctionPointer<NewObjectArray>(Marshal.ReadIntPtr(real, 688));
            FindClass findClass = Marshal.GetDelegateForFunctionPointer<FindClass>(Marshal.ReadIntPtr(real, 24));

            SetArray setArray = Marshal.GetDelegateForFunctionPointer<SetArray>(Marshal.ReadIntPtr(real, 696));

            DestroyJavaVM destroyVM = Marshal.GetDelegateForFunctionPointer<DestroyJavaVM>(Marshal.ReadIntPtr(realVmStruct, 12));

            //attach api start

            Java_sun_tools_attach_WindowsVirtualMachine_init(env, IntPtr.Zero);
            IntPtr stub = Java_sun_tools_attach_WindowsVirtualMachine_generateStub(env, IntPtr.Zero);

            long process = Java_sun_tools_attach_WindowsVirtualMachine_openProcess(env, IntPtr.Zero, pid);

            Console.WriteLine("exception " + checkException(env));


            IntPtr cmd = newStringUTF(env, "load");
            IntPtr pipeName = newStringUTF(env, "\\\\.\\pipe\\javatool22");

            IntPtr pathJStr = newStringUTF(env, path);
            IntPtr unknownBoolean = newStringUTF(env, "true");
            IntPtr argumentsJ = newStringUTF(env, arguments);

            Console.WriteLine("exception " + checkException(env));


            IntPtr clazz = findClass(env, "java/lang/String");

            IntPtr array = newObjectArray(env, 3, clazz, IntPtr.Zero);

            setArray(env, array, 0, pathJStr);
            setArray(env, array, 1, unknownBoolean);
            setArray(env, array, 2, argumentsJ);



            Java_sun_tools_attach_WindowsVirtualMachine_enqueue(env, IntPtr.Zero, 
                process, stub, cmd, pipeName, array);

            Console.WriteLine("exception " + checkException(env));
            printException(env);


            /*
            
            var pipe = new NamedPipeServerStream("javatool22");
            pipe.WaitForConnection();
            BinaryReader reader = new BinaryReader(pipe);
            char callback = reader.ReadChar();
      

            Console.WriteLine("pipe result " + callback);
            */

            Java_sun_tools_attach_WindowsVirtualMachine_closeProcess(env, IntPtr.Zero, process);


            int rr = destroyVM(vm);

            Console.WriteLine("destroyed vm: " + result);


        }

        [DllImport(@"D:\Program Files (x86)\Java\jdk1.8.0_181\jre\bin\client\jvm.dll")]
        public static extern int JNI_CreateJavaVM(ref IntPtr vm, ref IntPtr env, ref JavaVMInitArgs args);


        public delegate IntPtr StringDelegate(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string input);
        public delegate int GetStringLength(IntPtr env, IntPtr strin);

        public delegate byte ExceptionCheck(IntPtr env);
        public delegate void ExceptionDescribe(IntPtr env);

        public delegate IntPtr NewObjectArray(IntPtr env, int count, IntPtr clazz, IntPtr obj);
        public delegate IntPtr FindClass(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string input);

        public delegate IntPtr SetArray(IntPtr env, IntPtr array, int index, IntPtr obj);

        public delegate int DestroyJavaVM(IntPtr vm);

   
        public const string ATTACH_DLL = @"D:\Program Files (x86)\Java\jdk1.8.0_181\jre\bin\attach.dll";


        [DllImport(ATTACH_DLL)]
        public static extern void Java_sun_tools_attach_WindowsVirtualMachine_init
            (IntPtr env, IntPtr jobject);

        [DllImport(ATTACH_DLL)]
        public static extern IntPtr Java_sun_tools_attach_WindowsVirtualMachine_generateStub(IntPtr env, IntPtr jobject);

        [DllImport(ATTACH_DLL)]
        public static extern long Java_sun_tools_attach_WindowsVirtualMachine_openProcess
            (IntPtr env, IntPtr jobject, int param);

        [DllImport(ATTACH_DLL)]
        public static extern void Java_sun_tools_attach_WindowsVirtualMachine_closeProcess(
            IntPtr env, IntPtr jobject, long param);

        [DllImport(ATTACH_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern void Java_sun_tools_attach_WindowsVirtualMachine_enqueue(IntPtr env, IntPtr jobject, long process, 
            IntPtr jByteArrayStub, IntPtr jStringCmd, IntPtr jStringPipe, IntPtr jStringArrayArguments);
    }
}
