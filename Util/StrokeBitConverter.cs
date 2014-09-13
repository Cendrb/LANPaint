using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Util
{
    public static class StrokeBitConverter
    {
        public const int OWNER_NAME_BYTES_ARRAY_LENGTH = 128;
        public const int DRAWING_ATTRIBUTES_BYTES_ARRAY_LENGTH = COLOR_BYTES_ARRAY_LENGTH + sizeof(bool) + sizeof(double) + sizeof(double) + 1;
        public const int COLOR_BYTES_ARRAY_LENGTH = 4;
            

        public static SignedPointerStroke GetSignedPointerStroke(Stream source)
        {
            SignedStroke signed = GetSignedStroke(source);

            byte[] stayTimeBytes = new byte[sizeof(Int32)];
            source.Read(stayTimeBytes, 0, stayTimeBytes.Length);
            int stayTime = BitConverter.ToInt32(stayTimeBytes, 0);

            byte[] fadeTimeBytes = new byte[sizeof(Int32)];
            source.Read(fadeTimeBytes, 0, fadeTimeBytes.Length);
            int fadeTime = BitConverter.ToInt32(stayTimeBytes, 0);

            SignedPointerStroke stroke = new SignedPointerStroke(signed, stayTime, fadeTime);

            return stroke;
        }

        public static SignedStroke GetSignedStroke(Stream source)
        {
            byte[] ownerBytes = new byte[OWNER_NAME_BYTES_ARRAY_LENGTH];
            source.Read(ownerBytes, 0, ownerBytes.Length);

            byte[] idBytes = new byte[sizeof(UInt32)];
            source.Read(idBytes, 0, idBytes.Length);

            DrawingAttributes attributes = GetDrawingAttributes(source);

            StylusPointCollection points = new StylusPointCollection();

            byte[] numberOfPointsBytes = new byte[sizeof(UInt32)];
            source.Read(numberOfPointsBytes, 0, numberOfPointsBytes.Length);
            uint numberOfPoints = BitConverter.ToUInt32(numberOfPointsBytes, 0);

            for (uint x = numberOfPoints; x != 0; x--)
                points.Add(GetPoint(source));
            SignedStroke stroke = new SignedStroke(points, attributes);
            stroke.Owner = StringBitConverter.GetString(ownerBytes).Replace("\0", "");
            stroke.Id = BitConverter.ToUInt32(idBytes, 0);
            return stroke;
        }

        public static DrawingAttributes GetDrawingAttributes(Stream source)
        {
            DrawingAttributes attributes = new DrawingAttributes();

            // set color
            attributes.Color = GetColor(source);

            byte[] attributeBytes = new byte[18];
            source.Read(attributeBytes, 0, attributeBytes.Length);

            attributes.FitToCurve = BitConverter.ToBoolean(attributeBytes, 0);
            attributes.Width = BitConverter.ToDouble(attributeBytes, 1);
            attributes.Height = BitConverter.ToDouble(attributeBytes, 9);
            if (attributeBytes[17] == 0)
                attributes.StylusTip = StylusTip.Ellipse;
            else
                attributes.StylusTip = StylusTip.Rectangle;

            return attributes;
        }

        public static Color GetColor(Stream source)
        {
            byte[] colorBytes = new byte[4];

            source.Read(colorBytes, 0, colorBytes.Length);

            Color color = new Color();
            color.R = colorBytes[0];
            color.G = colorBytes[1];
            color.B = colorBytes[2];
            color.A = colorBytes[3];
            return color;
        }

        public static StylusPoint GetPoint(Stream source)
        {
            byte[] doubleBytes = new byte[sizeof(double)];

            source.Read(doubleBytes, 0, doubleBytes.Length);
            double x = BitConverter.ToDouble(doubleBytes, 0);

            source.Read(doubleBytes, 0, doubleBytes.Length);
            double y = BitConverter.ToDouble(doubleBytes, 0);

            return new StylusPoint(x, y);
        }

        public static byte[] GetBytes(SignedPointerStroke stroke)
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(stroke.StayTime));

            bytes.AddRange(BitConverter.GetBytes(stroke.FadeTime));

            bytes.AddRange(GetBytes(stroke as SignedStroke));

            return bytes.ToArray();
        }

        public static byte[] GetBytes(SignedStroke stroke)
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(StringBitConverter.GetBytes(stroke.Owner, OWNER_NAME_BYTES_ARRAY_LENGTH));

            bytes.AddRange(BitConverter.GetBytes(stroke.Id));

            bytes.AddRange(GetBytes(stroke.DrawingAttributes));

            StylusPointCollection points = stroke.StylusPoints;
            foreach (StylusPoint point in points)
            {
                bytes.AddRange(BitConverter.GetBytes(point.X));
                bytes.AddRange(BitConverter.GetBytes(point.Y));
            }
            return bytes.ToArray();
        }

        public static byte[] GetBytes(DrawingAttributes attributes)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(GetBytes(attributes.Color));
            bytes.AddRange(BitConverter.GetBytes(attributes.FitToCurve));
            bytes.AddRange(BitConverter.GetBytes(attributes.Width));
            bytes.AddRange(BitConverter.GetBytes(attributes.Height));
            byte stylusShape;
            if(attributes.StylusTip == StylusTip.Ellipse)
                stylusShape = 0;
            else
                stylusShape = 1;
            bytes.Add(stylusShape);
            return bytes.ToArray();
        }

        public static byte[] GetBytes(Color color)
        {
            byte[] bytes = new byte[4];
            bytes[0] = color.R;
            bytes[1] = color.G;
            bytes[2] = color.B;
            bytes[3] = color.A;
            return bytes;
        }
    }
}
