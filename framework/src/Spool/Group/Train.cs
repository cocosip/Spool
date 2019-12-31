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

        /// <summary>Name, format index. exp: _00001_, _00002_
        /// </summary>
        public string Name { get { return $"_{Index.ToString().PadLeft(5, '0')}_"; } }

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
