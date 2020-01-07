namespace Spool.Group
{
    /// <summary>Sequence train
    /// </summary>
    public class Train
    {
        public GroupPoolDescriptor Descriptor { get; set; }

        /// <summary>Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>Name, format index. exp: _000001_, _000002_
        /// </summary>
        public string Name { get { return $"_{Index.ToString().PadLeft(6, '0')}_"; } }

        /// <summary>Ctor
        /// </summary>
        public Train()
        {

        }

        /// <summary>Ctor
        /// </summary>
        public Train(int index, GroupPoolDescriptor descriptor)
        {
            Index = index;
            Descriptor = descriptor;
        }


    }
}
