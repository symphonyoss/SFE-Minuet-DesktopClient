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

namespace Paragon.Plugins
{
    public class BoundsSpecification
    {
        public BoundsSpecification()
        {
            Left = double.NaN;
            Top = double.NaN;
        }

        /// <summary>
        /// The X coordinate of the content or window.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// The Y coordinate of the content or window.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// The width of the content or window.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height of the content or window.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The minimum width of the content or window.
        /// </summary>
        public double MinWidth { get; set; }

        /// <summary>
        /// The minimum height of the content or window.
        /// </summary>
        public double MinHeight { get; set; }

        /// <summary>
        /// The maximum width of the content or window.
        /// </summary>
        public double MaxWidth { get; set; }

        /// <summary>
        /// The maximum height of the content or window.
        /// </summary>
        public double MaxHeight { get; set; }

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

            var other = obj as BoundsSpecification;

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

            if (Height != other.Height)
            {
                return false;
            }

            if (MaxHeight != other.MaxHeight)
            {
                return false;
            }

            if (MaxWidth != other.MaxWidth)
            {
                return false;
            }

            if (MinHeight != other.MinHeight)
            {
                return false;
            }

            if (MinWidth != other.MinWidth)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Since we have overriden equals, we must override GetHashCode. However, an objects hash code
        /// is meant to be immutable whilst also being linked to the result of the equals function.
        /// The easiest thing to do here is to return zero but we must never use these objects as a key
        /// in a hash table because it would be very inefficient.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}