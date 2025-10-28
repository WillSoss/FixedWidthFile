using System.Text;

namespace WillSoss.FixedWidthFile
{
    /// <summary>
    /// Determines the alignment of values in fixed-width columns. For left-to-right languages, 'Start' means left-aligned and 'End' means right-aligned.
    /// </summary>
    public enum ValueAlignment
    {
        /// <summary>
        /// Align value to the start of the column.
        /// </summary>
        Start,
        /// <summary>
        /// Align value to the end of the column.
        /// </summary>
        End
    }

    public class FixedWidthWriterBuilder
    {
        internal FixedWidthOptions Options { get; } = new();
        internal Dictionary<string, RecordBuilder> RecordTypes { get; } = new();
        internal Stream? Stream { get; private set; }

        public FixedWidthWriterBuilder WithDefaultPadding(char padding)
        {
            Options.DefaultPadding = padding;
            return this;
        }

        public FixedWidthWriterBuilder WithDefaultAlignment(ValueAlignment alignment)
        {
            Options.DefaultAlignment = alignment;
            return this;
        }

        public FixedWidthWriterBuilder WithEncoding(Encoding encoding)
        {
            Options.Encoding = encoding;
            return this;
        }

        public FixedWidthWriterBuilder WithRecordSeparator(string separator)
        {
            Options.RecordSeparator = separator;
            return this;
        }

        public FixedWidthWriterBuilder WithStream(Stream stream)
        {
            Stream = stream;
            return this;
        }

        public FixedWidthWriterBuilder AddRecordDefinition(Action<RecordBuilder> configure)
        {
            var fields = new RecordBuilder();

            configure(fields);

            if (RecordTypes.ContainsKey(fields.Type))
                throw new ArgumentException($"To create a multi-record file layout, record definitions must be given unique type names using {nameof(RecordBuilder.WithTypeName)}.");

            RecordTypes.Add(fields.Type, fields);

            return this;
        }

        public FixedWidthWriter Build()
        {
            return new FixedWidthWriter(this);
        }
    }
}
