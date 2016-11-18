﻿/**
Copyright 2014-2016 Robert McNeel and Associates

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**/

namespace ccl.ShaderNodes.Sockets
{
	public class SocketBase
	{
		internal ShaderNode Parent { get; set; }

		public string Name { get; set; }

		public string XmlName => Name.Replace(' ', '_').ToLowerInvariant();

		public void Connect(SocketBase to)
		{
			to.ConnectionFrom = this;
		}

		internal SocketBase(ShaderNode parentNode, string name)
		{
			Parent = parentNode;
			Name = name;
		}

		internal SocketBase ConnectionFrom { get; set; }

		/// <summary>
		/// Get string containing node name, type and socket name
		/// </summary>
		public string Path => $"{Parent.Name}({Parent.Type}):{Name}";

		/// <summary>
		/// Get the C# connection code into this socket
		/// </summary>
		public string ConnectCode => ConnectionFrom != null ? $"{ConnectionFrom.Parent.Name}.outs.{ConnectionFrom.Name}.Connect({Parent.Name}.ins.{Name});" : "";

		public string ConnectTag => ConnectionFrom != null ? $"<connect to=\"{Parent.Name} {XmlName}\" from=\"{ConnectionFrom.Parent.Name} {ConnectionFrom.XmlName}\" />": "";

		/// <summary>
		/// Remove connections
		/// </summary>
		public void ClearConnections()
		{
			ConnectionFrom = null;
		}
	}
}
