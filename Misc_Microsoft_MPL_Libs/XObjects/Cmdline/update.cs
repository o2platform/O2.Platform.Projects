namespace XObjectsGenerator
{
	using S = global::System;
	using IO = global::System.IO;
	using T = global::System.Text;

	internal class Update: S.IDisposable
	{
		private IO.MemoryStream stream = new IO.MemoryStream();

		private string filename;

		private T.Encoding encoding;

		private IO.TextWriter writer;

		public IO.TextWriter Writer
		{
			get
			{
				return this.writer;
			}
		}

		public Update(string filename, T.Encoding encoding)
		{
			this.filename = filename;
			this.encoding = encoding;
			this.writer = new IO.StreamWriter(this.stream, encoding);
		}

		public bool Close()
		{
			this.writer.Close();
			var memoryString = new IO.StreamReader(
					new IO.MemoryStream(this.stream.ToArray()), this.encoding).
				ReadToEnd();
			string fileString = "";
			using (var file = new IO.FileStream(
				this.filename, IO.FileMode.OpenOrCreate))
			{
				using (var fileReader = new IO.StreamReader(file))
				{
					fileString = fileReader.ReadToEnd();
				}
			}
			if (memoryString != fileString)
			{
				using (var file = new IO.FileStream(this.filename, IO.FileMode.Create))
				{
					using (var fileWriter = new IO.StreamWriter(file, this.encoding))
					{
						fileWriter.Write(memoryString);
						file.SetLength(file.Position);
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			this.Close();
		}

		#endregion
	}
}
