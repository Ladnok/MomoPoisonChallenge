using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

internal static class WatcherUtility
{
    /// <summary>
    /// Create a new MemoryWatcher
    /// </summary>
    /// <typeparam name="T">Type of the value to be watched.</typeparam>
    /// <param name="name">MemoryWatcher Name</param>
    /// <param name="pointer">Pointer to memory address</param>
    /// <param name="timeSpan">Update interval (defaults to 10ms)</param>
    /// <param name="enabled">Enable or disable the Watcher (enabled by default)</param>
    /// <param name="OnChanged">Method reference to attach to OnChanged event</param>
    /// <returns>A new MemoryWatcher object.</returns>
    public static MemoryWatcher CreateWatcher<T>(string name, IntPtr pointer, TimeSpan? timeSpan, bool? enabled, MemoryWatcher<T>.DataChangedEventHandler OnChanged = null) where T : struct
    {
        var watcher = new MemoryWatcher<T>(pointer)
        {
            UpdateInterval = timeSpan ?? TimeSpan.FromMilliseconds(10),
            Enabled = enabled ?? true,
            Name = name
        };

        if (OnChanged != null)
        {
            watcher.OnChanged += OnChanged;
        }

        return watcher;
    }

    /// <summary>
    /// Create a new MemoryWatcher
    /// </summary>
    /// <typeparam name="T">Type of the value to be watched.</typeparam>
    /// <param name="name">MemoryWatcher Name</param>
    /// <param name="pointer">Pointer to memory address</param>
    /// <param name="timeSpan">Update interval (defaults to 10ms)</param>
    /// <param name="enabled">Enable or disable the Watcher (enabled by default)</param>
    /// <param name="OnChanged">Method reference to attach to OnChanged event</param>
    /// <returns>A new MemoryWatcher object.</returns>
    public static MemoryWatcher CreateWatcher<T>(string name, DeepPointer pointer, TimeSpan? timeSpan, bool? enabled, MemoryWatcher<T>.DataChangedEventHandler OnChanged = null) where T : struct
    {
        var watcher = new MemoryWatcher<T>(pointer)
        {
            UpdateInterval = timeSpan ?? TimeSpan.FromMilliseconds(10),
            Enabled = enabled ?? true,
            Name = name
        };

        if (OnChanged != null)
        {
            watcher.OnChanged += OnChanged;
        }

        return watcher;
    }

    /// <summary>
    /// Create a new MemoryWatcher
    /// </summary>
    /// <param name="name">MemoryWatcher Name</param>
    /// <param name="pointer">Pointer to memory address</param>
    /// <param name="OnChanged">Method reference to attach to OnChanged event</param>
    public static MemoryWatcher CreateWatcher<T>(string name, IntPtr pointer, MemoryWatcher<T>.DataChangedEventHandler OnChanged) where T : struct => CreateWatcher<T>(name, pointer, null, null, OnChanged);

    /// <summary>
    /// Create a new MemoryWatcher
    /// </summary>
    /// <param name="name">MemoryWatcher Name</param>
    /// <param name="pointer">Pointer to memory address</param>
    /// <param name="OnChanged">Method reference to attach to OnChanged event</param>
    public static MemoryWatcher CreateWatcher<T>(string name, DeepPointer pointer, MemoryWatcher<T>.DataChangedEventHandler OnChanged) where T : struct => CreateWatcher<T>(name, pointer, null, null, OnChanged);

    /// <summary>
    /// Create a new MemoryWatcher
    /// </summary>
    /// <param name="name">MemoryWatcher Name</param>
    /// <param name="pointer">Pointer to memory address</param>
    /// <param name="enabled"><c>Enable</c> or <c>Disable</c> Watcher</param>
    /// <param name="OnChanged">Method reference to attach to OnChanged event</param>
    public static MemoryWatcher CreateWatcher<T>(string name, IntPtr pointer, bool enabled = true, MemoryWatcher<T>.DataChangedEventHandler OnChanged = null) where T : struct => CreateWatcher<T>(name, pointer, null, enabled, OnChanged);

    /// <summary>
    /// Create a new MemoryWatcher
    /// </summary>
    /// <param name="name">MemoryWatcher Name</param>
    /// <param name="pointer">Pointer to memory address</param>
    /// <param name="enabled"><c>Enable</c> or <c>Disable</c> Watcher</param>
    /// <param name="OnChanged">Method reference to attach to OnChanged event</param>
    public static MemoryWatcher CreateWatcher<T>(string name, DeepPointer pointer, bool enabled = true, MemoryWatcher<T>.DataChangedEventHandler OnChanged = null) where T : struct => CreateWatcher<T>(name, pointer, null, enabled, OnChanged);

    /// <summary>
    /// Create an IntPtr for the process with the offsets (includes the base).
    /// </summary>
    /// <param name="process">The process to dereference.</param>
    /// <param name="offsets">Offsets, including the base, of the pointer.</param>
    /// <returns>A reference to an IntPtr, or <c>IntPtr.Zero</c> if invalid.</returns>
    public static IntPtr CreateIntPointer(Process process, params int[] offsets)
    {
        if (offsets.Length == 1)
        {
            return IntPtr.Add(process.MainModule.BaseAddress, offsets.First());
        }
        else if (offsets.Length > 1)
        {
            var DeepPointer = new DeepPointer(offsets.First(), offsets.Skip(1).Take(offsets.Length - 2).ToArray());
            if (DeepPointer != null)
            {
                return IntPtr.Add((IntPtr)DeepPointer.Deref<int>(process), offsets.Last());
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Create a DeepPointer for the process with the offsets (includes the base)<br/>
    /// </summary>
    /// <param name="process">The process to dereference.</param>
    /// <param name="offsets">Offsets, including the base, of the pointer.</param>
    /// <returns>A DeepPointer, or <c>null</c> if invalid.</returns>
    public static DeepPointer CreateDeepPointer(Process process, params int[] offsets)
    {
        if (offsets.Length > 0)
        {
            return new DeepPointer(process.MainModule.BaseAddress + offsets[0], offsets.Skip(1).ToArray());
        }

        return null;
    }

    /// <summary>
    /// Writes a value of a specified type to the memory of a target process at the given address.
    /// </summary>
    /// <typeparam name="T">The type of the value to write. Must be a value type (struct).</typeparam>
    /// <param name="process">The target process to write the value to.</param>
    /// <param name="pointer">The memory address where the value will be written.</param>
    /// <param name="value">The value to be written to the process's memory.</param>
    /// <returns>
    /// <c>true</c> if the value was successfully written to the process's memory.<br/>
    /// <c>false</c> if the action failed.
    /// </returns>
    public static bool WriteValue<T>(Process process, IntPtr pointer, T value) where T : struct
    {
        if (process != null && !process.HasExited)
        {
            return process.WriteValue<T>(pointer, value);
        }

        return false;
    }

    /// <summary>
    /// Reads a value of a specified type from the memory of a target process at the given address.
    /// </summary>
    /// <typeparam name="T">The type of the value to read. Must be a value type (struct).</typeparam>
    /// <param name="process">The target process from which to read the value.</param>
    /// <param name="pointer">The memory address from which the value will be read.</param>
    /// <returns>
    /// The value read from the process's memory, or the default value of type T if the process is null or if the value type is not a valid struct.
    /// </returns>
    public static T ReadValue<T>(Process process, IntPtr pointer) where T : struct => process?.ReadValue<T>(pointer) ?? default;
}