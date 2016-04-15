//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System.Drawing;

namespace Paragon.Runtime.Kernel.Windowing
{
    public class Bounds
    {
        public int Height;
        public int Left;
        public int Top;
        public int Width;

        public static Bounds FromRectangle(Rectangle r)
        {
            return new Bounds {Left = r.Left, Top = r.Top, Width = r.Width, Height = r.Height};
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            var other = obj as Bounds;
            if (other == null)
            {
                return false;
            }

            if (Left != other.Left)
            {
                return false;
            }

            if (Top != other.Top)
            {
                return false;
            }

            if (Width != other.Width)
            {
                return false;
            }

            return Height == other.Height;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}