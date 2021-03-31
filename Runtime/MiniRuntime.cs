namespace System.Runtime.CompilerServices
{
    // A class responsible for running static constructors. The compiler will call into this
    // code to ensure static constructors run and that they only run once.
    [System.Runtime.InteropServices.McgIntrinsics]
    internal static class ClassConstructorRunner
    {
        private static unsafe IntPtr CheckStaticClassConstructionReturnNonGCStaticBase(ref StaticClassConstructionContext context, IntPtr nonGcStaticBase)
        {
            CheckStaticClassConstruction(ref context);
            return nonGcStaticBase;
        }

        private static unsafe void CheckStaticClassConstruction(ref StaticClassConstructionContext context)
        {
            // Very simplified class constructor runner. In real world, the class constructor runner
            // would need to be able to deal with potentially multiple threads racing to initialize
            // a single class, and would need to be able to deal with potential deadlocks
            // between class constructors.

            if (context.initialized == 1)
                return;

            context.initialized = 1;

            // Run the class constructor.
            Call<int>(context.cctorMethodAddress);
        }

        // This is a special compiler intrinsic that calls method pointed to by pfn.
        [System.Runtime.CompilerServices.Intrinsic]
        public static extern T Call<T>(System.IntPtr pfn);
    }

    // This data structure is a contract with the compiler. It holds the address of a static
    // constructor and a flag that specifies whether the constructor already executed.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct StaticClassConstructionContext
    {
        // Pointer to the code for the static class constructor method. This is initialized by the
        // binder/runtime.
        public IntPtr cctorMethodAddress;

        // Initialization state of the class. This is initialized to 0. Every time managed code checks the
        // cctor state the runtime will call the classlibrary's CheckStaticClassConstruction with this context
        // structure unless initialized == 1. This check is specific to allow the classlibrary to store more
        // than a binary state for each cctor if it so desires.
        public int initialized;
    }
}
