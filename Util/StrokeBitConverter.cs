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
        public const byte SIGNED_STROKE_SIGNAL = 0;
        public const byte SIGNED_POINTER_STROKE_SIGNAL = 1;
        public const byte STROKES_END = 69;

        public const int OWNER_NAME_BYTES_ARRAY_LENGTH = 128;
        public const int DRAWING_ATTRIBUTES_BYTES_ARRAY_LENGTH = COLOR_BYTES_ARRAY_LENGTH + sizeof(bool) + sizeof(double) + sizeof(double) + 1;
        public const int COLOR_BYTES_ARRAY_LENGTH = 4;

        #region Deserialize
        public static SignedPointerStroke GetSignedPointerStroke(Stream source)
        {
            byte[] stayTimeBytes = new byte[sizeof(Int32)];
            source.Read(stayTimeBytes, 0, stayTimeBytes.Length);
            int stayTime = BitConverter.ToInt32(stayTimeBytes, 0);

            byte[] fadeTimeBytes = new byte[sizeof(Int32)];
            source.Read(fadeTimeBytes, 0, fadeTimeBytes.Length);
            int fadeTime = BitConverter.ToInt32(stayTimeBytes, 0);

            SignedStroke signed = GetSignedStroke(source);

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
            int numberOfPoints = BitConverter.ToInt32(numberOfPointsBytes, 0);

            for (int x = numberOfPoints; x != 0; x--)
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

            byte[] fitToCurveBytes = new byte[sizeof(bool)];
            source.Read(fitToCurveBytes, 0, fitToCurveBytes.Length);
            attributes.FitToCurve = BitConverter.ToBoolean(fitToCurveBytes, 0);

            byte[] widthBytes = new byte[sizeof(double)];
            source.Read(widthBytes, 0, widthBytes.Length);
            attributes.Width = BitConverter.ToDouble(widthBytes,0);

            byte[] heightBytes = new byte[sizeof(double)];
            source.Read(heightBytes, 0, heightBytes.Length);
            attributes.Height = BitConverter.ToDouble(heightBytes, 0);

            byte[] stylusBytes = new byte[1];
            source.Read(stylusBytes, 0, stylusBytes.Length);
            if (stylusBytes[0] == 0)
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
            byte[] xBytes = new byte[sizeof(double)];
            source.Read(xBytes, 0, xBytes.Length);
            double x = BitConverter.ToDouble(xBytes, 0);

            byte[] yBytes = new byte[sizeof(double)];
            source.Read(yBytes, 0, yBytes.Length);
            double y = BitConverter.ToDouble(yBytes, 0);

            return new StylusPoint(x, y);
        }
        #endregion

        #region Serialize
        public static void Serialize(Stream target, SignedPointerStroke stroke)
        {
            target.Write(BitConverter.GetBytes(stroke.StayTime), 0, sizeof(Int32));

            target.Write(BitConverter.GetBytes(stroke.FadeTime), 0, sizeof(Int32));

            Serialize(target, stroke as SignedStroke);
        }

        public static void Serialize(Stream target, SignedStroke stroke)
        {
            byte[] ownerBytes = StringBitConverter.GetBytes(stroke.Owner, OWNER_NAME_BYTES_ARRAY_LENGTH);
            target.Write(ownerBytes, 0, ownerBytes.Length);

            target.Write(BitConverter.GetBytes(stroke.Id), 0, sizeof(Int32));

            Serialize(target, stroke.DrawingAttributes);

            StylusPointCollection points = stroke.StylusPoints;

            target.Write(BitConverter.GetBytes(points.Count), 0, sizeof(Int32));

            foreach (StylusPoint point in points)
                Serialize(target, point);
        }

        public static void Serialize(Stream target, StylusPoint point)
        {
            target.Write(BitConverter.GetBytes(point.X), 0, sizeof(double));
            target.Write(BitConverter.GetBytes(point.Y), 0, sizeof(double));
        }

        public static void Serialize(Stream target, DrawingAttributes attributes)
        {
            Serialize(target, attributes.Color);

            target.Write(BitConverter.GetBytes(attributes.FitToCurve), 0, sizeof(bool));
            target.Write(BitConverter.GetBytes(attributes.Width), 0, sizeof(double));
            target.Write(BitConverter.GetBytes(attributes.Height), 0, sizeof(double));

            byte stylusShape;
            if(attributes.StylusTip == StylusTip.Ellipse)
                stylusShape = 0;
            else
                stylusShape = 1;
            target.WriteByte(stylusShape);
        }

        public static void Serialize(Stream target, Color color)
        {
            byte[] bytes = new byte[4];
            bytes[0] = color.R;
            bytes[1] = color.G;
            bytes[2] = color.B;
            bytes[3] = color.A;
            target.Write(bytes, 0, bytes.Length);
        }
        #endregion
    }
}
