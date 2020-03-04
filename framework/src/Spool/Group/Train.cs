namespace Spool.Group
{
    /// <summary>Sequence train
    /// </summary>
    public class Train
    {
        /// <summary>Index
        /// </summary>
        public int Index { get; private set; }

        /// <summary>Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>Descriptor
        /// </summary>
        public SpoolGroupDescriptor Descriptor { get; private set; }

        /// <summary>Path
        /// </summary>
        public string Path { get; private set; }

        public Train(int index, SpoolGroupDescriptor descriptor)
        {
            Index = index;
            Descriptor = descriptor;
            Name = FormatName(index);
            Path = GetPath(descriptor, Name);
        }

        private string FormatName(int index)
        {
            return $"_{index.ToString().PadLeft(6, '0')}_";
        }

        private string GetPath(SpoolGroupDescriptor descriptor, string name)
        {
            return System.IO.Path.Combine(descriptor.Path, name);
        }

    }
}
