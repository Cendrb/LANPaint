using System;
using System.Collections.Generic;
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
            

        public static SignedPointerStroke GetSignedPointerStroke(byte[] bytes)
        {
            SignedStroke signed = GetSignedStroke(bytes.Skip(sizeof(int) * 2).ToArray());

            int stayTime = BitConverter.ToInt32(bytes, 0);
            int fadeTime = BitConverter.ToInt32(bytes, sizeof(int));

            SignedPointerStroke stroke = new SignedPointerStroke(signed, stayTime, fadeTime);

            return stroke;
        }

        public static SignedStroke GetSignedStroke(byte[] bytes)
        {
            StylusPointCollection points = new StylusPointCollection();
            DrawingAttributes attributes = null;

            List<byte> listBytes = new List<byte>(bytes);

            int takenBytes = 0;

            byte[] ownerBytes = listBytes.Take(OWNER_NAME_BYTES_ARRAY_LENGTH).ToArray();
            takenBytes += OWNER_NAME_BYTES_ARRAY_LENGTH;

            byte[] idBytes = listBytes.Skip(takenBytes).Take(sizeof(UInt32)).ToArray();
            takenBytes += sizeof(UInt32);

            attributes = GetDrawingAttributes(listBytes.Skip(takenBytes).Take(DRAWING_ATTRIBUTES_BYTES_ARRAY_LENGTH).ToArray());
            takenBytes += DRAWING_ATTRIBUTES_BYTES_ARRAY_LENGTH;

            byte[] pointsBytes = listBytes.Skip(takenBytes).ToArray();
            for (int x = 0; x < pointsBytes.Length; x += 16)
                points.Add(GetPoint(pointsBytes, x));
            SignedStroke stroke = new SignedStroke(points, attributes);
            stroke.Owner = StringBitConverter.GetString(ownerBytes).Replace("\0", "");
            stroke.Id = BitConverter.ToUInt32(idBytes, 0);
            return stroke;
        }

        public static DrawingAttributes GetDrawingAttributes(byte[] bytes)
        {
            DrawingAttributes attributes = new DrawingAttributes();

            // set color
            attributes.Color = GetColor(bytes, 0);
            attributes.FitToCurve = BitConverter.ToBoolean(bytes, 4);
            attributes.Width = BitConverter.ToDouble(bytes, 5);
            attributes.Height = BitConverter.ToDouble(bytes, 13);
            if (bytes[21] == 0)
                attributes.StylusTip = StylusTip.Ellipse;
            else
                attributes.StylusTip = StylusTip.Rectangle;

            return attributes;
        }

        public static Color GetColor(byte[] bytes, int startindex)
        {
            Color color = new Color();
            color.R = bytes[0 + startindex];
            color.G = bytes[1 + startindex];
            color.B = bytes[2 + startindex];
            color.A = bytes[3 + startindex];
            return color;
        }

        public static StylusPoint GetPoint(byte[] bytes, int startIndex)
        {
            double x = BitConverter.ToDouble(bytes, 0 + startIndex);
            double y = BitConverter.ToDouble(bytes, 8 + startIndex);

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
