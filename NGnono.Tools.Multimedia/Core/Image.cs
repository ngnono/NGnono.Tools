using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using Common.Logging;

namespace NGnono.Tools.Multimedia.Core
{
    /// <summary>
    /// 
    /// </summary>
    public enum ThumbMode
    {
        /// <summary>
        /// 
        /// </summary>
        NHW,
        /// <summary>
        /// 
        /// </summary>
        HW,
        /// <summary>
        /// 
        /// </summary>
        H,
        /// <summary>
        /// 
        /// </summary>
        W,
        /// <summary>
        /// 
        /// </summary>
        Cut
    }

    internal class ThumbnailRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public int? Width;

        public int? Height;

        public string OriginalImageFullName { get; set; }

        public Dictionary<int, string> ExifInfos { get; set; }

        public ThumbMode ThumbMode { get; set; }

        public int ImageQuality { get; set; }

        public string SaveImageFullName { get; set; }
    }

    internal class ThumbnailResult
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public long ContentSize { get; set; }
    }

    internal interface IThumbnailGeneraterProvider
    {
        ThumbnailResult Generater(ThumbnailRequest info);
    }

    internal class Net4ThumbnailGeneraterProvider : IThumbnailGeneraterProvider
    {
        public ThumbnailResult Generater(ThumbnailRequest info)
        {
            throw new NotImplementedException();
        }
    }

    internal class ImageMagick4ThumbnailGeneraterProvider : IThumbnailGeneraterProvider
    {
        private readonly string _pathImageMagick;
        private readonly ILog _logger;

        public ImageMagick4ThumbnailGeneraterProvider()
            : this(Path.GetFullPath(ConfigurationManager.AppSettings["imagemegickExePath"]))
        {
        }

        public ImageMagick4ThumbnailGeneraterProvider(string fullPath)
        {
            _logger = LogManager.GetLogger(GetType());
            _pathImageMagick = fullPath;
        }

        public ThumbnailResult Generater(ThumbnailRequest info)
        {
            _logger.Debug("begin image ");
            try
            {
                info.OriginalImageFullName = System.IO.Path.GetFullPath(info.OriginalImageFullName);
                info.SaveImageFullName = System.IO.Path.GetFullPath(info.SaveImageFullName);

                if (info.Height == null || info.Height.Value == 0)
                {
                    Make(info.ImageQuality, info.OriginalImageFullName, info.SaveImageFullName, info.Width ?? 0);
                }
                else
                {
                    Make(info.ImageQuality, info.OriginalImageFullName, info.SaveImageFullName, info.Width ?? 0, info.Height.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            _logger.Debug("end image");

            return new ThumbnailResult { ContentSize = 0, Height = info.Height ?? 0, Width = info.Width ?? 0 };
        }

        private void Make(int quality, string originalImagePath, string thumbnailPath, int width, int height)
        {
            var sbFileArgs = new StringBuilder()
                        .Append(String.Format("{0}", originalImagePath))
                  .Append(@" -intent relative")
                       .AppendFormat(@" -resize {0}x{1} ", width, height)
                         .Append(@" -unsharp .5x.5+.5+0 ")
                //        .Append(@" -depth 8 ")
                //     .Append(@" -strip")
                        .Append(String.Format(" -quality {0} ", quality.ToString(CultureInfo.InvariantCulture)))
                        .Append(thumbnailPath);
            var fileArgs = sbFileArgs.ToString();
            _logger.Debug(fileArgs);
            CallImageMagick(fileArgs);
        }

        private void Make(int quality, string originalImagePath, string thumbnailPath, int width)
        {
            var sbFileArgs = new StringBuilder()
                        .Append(String.Format("{0}", originalImagePath))
                  .Append(@" -intent relative")
                       .AppendFormat(@" -resize {0}x ", width)
                         .Append(@" -unsharp .5x.5+.5+0 ")
                //        .Append(@" -depth 8 ")
                //     .Append(@" -strip")
                        .Append(String.Format(" -quality {0} ", quality.ToString(CultureInfo.InvariantCulture)))
                        .Append(thumbnailPath);
            var fileArgs = sbFileArgs.ToString();
            _logger.Debug(fileArgs);
            CallImageMagick(fileArgs);
            /*
           ImageMagickNET.Image tempImage = new ImageMagickNET.Image();
           tempImage.Read(originalImagePath);
           Geometry geo = new Geometry();
           geo.Width((uint)width);
           geo.Aspect();
           tempImage.Resize(geo);
           tempImage.Unsharpmask(1.5, 1, 0.7, 0.02);   //must keep value like this
           tempImage.Write(thumbnailPath);
           return;
             * */
        }

        private void CallImageMagick(string fileArgs)
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = fileArgs,
                FileName = _pathImageMagick,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true
            };

            using (var exeProcess = Process.Start(startInfo))//Process.Start(System.IO.Path.Combine(pathImageMagick,appImageMagick),fileArgs))
            {
                exeProcess.WaitForExit();
                exeProcess.Close();
            }
        }
    }

    #region exif

    internal class ExifHelper
    {
        #region Helper

        protected static void ProcessExifInfo(Image image, IDictionary<int, string> exifInfos)
        {
            ReadExif(exifInfos, image, ExifFileds.ImageWidth);
            ReadExif(exifInfos, image, ExifFileds.ImageHeight);
            ReadExif(exifInfos, image, ExifFileds.DateTimeOriginal);
            ReadExif(exifInfos, image, ExifFileds.Software);
            ReadExif(exifInfos, image, ExifFileds.ExposureTime);
            ReadExif(exifInfos, image, ExifFileds.ExposureProgram);
            ReadExif(exifInfos, image, ExifFileds.Make);
            ReadExif(exifInfos, image, ExifFileds.Model);
            ReadExif(exifInfos, image, ExifFileds.FocalLength);
            ReadExif(exifInfos, image, ExifFileds.ApertureValue);
            ReadExif(exifInfos, image, ExifFileds.MeteringMode);
            ReadExif(exifInfos, image, ExifFileds.ISOSpeedRatings);

            exifInfos.Add((int)ExifFileds.Flash, ExifTags.ReadTag(image, ExifFileds.Flash) == "关闭" ? "关闭" : "开启");
        }

        protected static void ReadExif(IDictionary<int, string> exifInfos, Image image, ExifFileds exifType)
        {
            exifInfos.Add((int)exifType, ExifTags.ReadTag(image, exifType));
        }

        #endregion
    }

    #region ExifTag

    public sealed class ExifTag
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }

        public ExifTag(int id, string fieldName, string description)
        {
            Id = id;
            Description = description;
            FieldName = fieldName;
        }

        public override string ToString()
        {
            return String.Format("Field Id:\t{0}\nField Name:\t{1}\nDescription:\t{2}\nField Value:\t{3}",
                                 Id, FieldName, Description, Value);
        }
    }

    #endregion

    #region SupportedTags

    internal class SupportedTags : List<ExifTag>
    {
        private static SupportedTags _inst;

        private SupportedTags()
        {
            Add(new ExifTag(0x100, "ImageWidth", "Image width"));
            Add(new ExifTag(0x101, "ImageHeight", "Image height"));
            Add(new ExifTag(0x0, "GPSVersionID", "GPS tag version"));
            Add(new ExifTag(0x5, "GPSAltitudeRef", "Altitude reference"));
            Add(new ExifTag(0x111, "StripOffsets", "Image data location"));
            Add(new ExifTag(0x116, "RowsPerStrip", "Number of rows per strip"));
            Add(new ExifTag(0x117, "StripByteCounts", "Bytes per compressed strip"));
            Add(new ExifTag(0xA002, "PixelXDimension", "Valid image width"));
            Add(new ExifTag(0xA003, "PixelYDimension", "Valid image height"));
            Add(new ExifTag(0x102, "BitsPerSample", "Number of bits per component"));
            Add(new ExifTag(0x103, "Compression", "Compression scheme"));
            Add(new ExifTag(0x106, "PhotometricInterpretation", "Pixel composition"));
            Add(new ExifTag(0x112, "Orientation", "Orientation of image"));
            Add(new ExifTag(0x115, "SamplesPerPixel", "Number of components"));
            Add(new ExifTag(0x11C, "PlanarConfiguration", "Image data arrangement"));
            Add(new ExifTag(0x212, "YCbCrSubSampling", "Subsampling ratio of Y to C"));
            Add(new ExifTag(0x213, "YCbCrPositioning", "Y and C positioning"));
            Add(new ExifTag(0x128, "ResolutionUnit", "Unit of X and Y resolution"));
            Add(new ExifTag(0x12D, "TransferFunction", "Transfer function"));
            Add(new ExifTag(0xA001, "ColorSpace", "Color space information"));
            Add(new ExifTag(0x8822, "ExposureProgram", "Exposure program"));
            Add(new ExifTag(0x8827, "ISOSpeedRatings", "ISO speed rating"));
            Add(new ExifTag(0x9207, "MeteringMode", "Metering mode"));
            Add(new ExifTag(0x9208, "LightSource", "Light source"));
            Add(new ExifTag(0x9209, "Flash", "Flash"));
            Add(new ExifTag(0x9214, "SubjectArea", "Subject area"));
            Add(new ExifTag(0xA210, "FocalPlaneResolutionUnit", "Focal plane resolution unit"));
            Add(new ExifTag(0xA214, "SubjectLocation", "Subject location"));
            Add(new ExifTag(0xA217, "SensingMethod", "Sensing method"));
            Add(new ExifTag(0xA401, "CustomRendered", "Custom image processing"));
            Add(new ExifTag(0xA402, "ExposureMode", "Exposure mode"));
            Add(new ExifTag(0xA403, "WhiteBalance", "White balance"));
            Add(new ExifTag(0xA405, "FocalLengthIn35mmFilm", "Focal length in 35 mm film"));
            Add(new ExifTag(0xA406, "SceneCaptureType", "Scene capture type"));
            Add(new ExifTag(0xA408, "Contrast", "Contrast"));
            Add(new ExifTag(0xA409, "Saturation", "Saturation"));
            Add(new ExifTag(0xA40A, "Sharpness", "Sharpness"));
            Add(new ExifTag(0xA40C, "SubjectDistanceRange", "Subject distance range"));
            Add(new ExifTag(0x1E, "GPSDifferential", "GPS differential correction"));
            Add(new ExifTag(0x9201, "ShutterSpeedValue", "Shutter speed"));
            Add(new ExifTag(0x9203, "BrightnessValue", "Brightness"));
            Add(new ExifTag(0x9204, "ExposureBiasValue", "Exposure bias"));
            Add(new ExifTag(0x201, "JPEGInterchangeFormat", "Offset to JPEG SOI"));
            Add(new ExifTag(0x202, "JPEGInterchangeFormatLength", "Bytes of JPEG data"));
            Add(new ExifTag(0x11A, "XResolution", "Image resolution in width direction"));
            Add(new ExifTag(0x11B, "YResolution", "Image resolution in height direction"));
            Add(new ExifTag(0x13E, "WhitePoint", "White point chromaticity"));
            Add(new ExifTag(0x13F, "PrimaryChromaticities", "Chromaticities of primaries"));
            Add(new ExifTag(0x211, "YCbCrCoefficients", "Color space transformation matrix coefficients"));
            Add(new ExifTag(0x214, "ReferenceBlackWhite", "Pair of black and white reference values"));
            Add(new ExifTag(0x9102, "CompressedBitsPerPixel", "Image compression mode"));
            Add(new ExifTag(0x829A, "ExposureTime", "Exposure time"));
            Add(new ExifTag(0x829D, "FNumber", "F number"));
            Add(new ExifTag(0x9202, "ApertureValue", "Aperture"));
            Add(new ExifTag(0x9205, "MaxApertureValue", "Maximum lens aperture"));
            Add(new ExifTag(0x9206, "SubjectDistance", "Subject distance"));
            Add(new ExifTag(0x920A, "FocalLength", "Lens focal length"));
            Add(new ExifTag(0xA20B, "FlashEnergy", "Flash energy"));
            Add(new ExifTag(0xA20E, "FocalPlaneXResolution", "Focal plane X resolution"));
            Add(new ExifTag(0xA20F, "FocalPlaneYResolution", "Focal plane Y resolution"));
            Add(new ExifTag(0xA215, "ExposureIndex", "Exposure index"));
            Add(new ExifTag(0xA404, "DigitalZoomRatio", "Digital zoom ratio"));
            Add(new ExifTag(0xA407, "GainControl", "Gain control"));
            Add(new ExifTag(0x2, "GPSLatitude", "Latitude"));
            Add(new ExifTag(0x4, "GPSLongitude", "Longitude"));
            Add(new ExifTag(0x6, "GPSAltitude", "Altitude"));
            Add(new ExifTag(0x7, "GPSTimeStamp", "GPS time (atomic clock)"));
            Add(new ExifTag(0xB, "GPSDOP", "Measurement precision"));
            Add(new ExifTag(0xD, "GPSSpeed", "Speed of GPS receiver"));
            Add(new ExifTag(0xF, "GPSTrack", "Direction of movement"));
            Add(new ExifTag(0x11, "GPSImgDirection", "Direction of image"));
            Add(new ExifTag(0x14, "GPSDestLatitude", "Latitude of destination"));
            Add(new ExifTag(0x16, "GPSDestLongitude", "Longitude of destination"));
            Add(new ExifTag(0x18, "GPSDestBearing", "Bearing of destination"));
            Add(new ExifTag(0x1A, "GPSDestDistance", "Distance to destination"));
            Add(new ExifTag(0x132, "DateTime", "File change date and time"));
            Add(new ExifTag(0x10E, "ImageDescription", "Image title"));
            Add(new ExifTag(0x10F, "Make", "Image input equipment manufacturer"));
            Add(new ExifTag(0x110, "Model", "Image input equipment model"));
            Add(new ExifTag(0x131, "Software", "Software used"));
            Add(new ExifTag(0x13B, "Artist", "Person who created the image"));
            Add(new ExifTag(0x8298, "Copyright", "Copyright holder"));
            Add(new ExifTag(0xA004, "RelatedSoundFile", "Related audio file"));
            Add(new ExifTag(0x9003, "DateTimeOriginal", "Date and time of original data generation"));
            Add(new ExifTag(0x9004, "DateTimeDigitized", "Date and time of digital data generation"));
            Add(new ExifTag(0x9290, "SubSecTime", "DateTime subseconds"));
            Add(new ExifTag(0x9291, "SubSecTimeOriginal", "DateTimeOriginal subseconds"));
            Add(new ExifTag(0x9292, "SubSecTimeDigitized", "DateTimeDigitized subseconds"));
            Add(new ExifTag(0xA420, "ImageUniqueID", "Unique image ID"));
            Add(new ExifTag(0x8824, "SpectralSensitivity", "Spectral sensitivity"));
            Add(new ExifTag(0x1, "GPSLatitudeRef", "North or South Latitude"));
            Add(new ExifTag(0x3, "GPSLongitudeRef", "East or West Longitude"));
            Add(new ExifTag(0x8, "GPSSatellites", "GPS satellites used for measurement"));
            Add(new ExifTag(0x9, "GPSStatus", "GPS receiver status"));
            Add(new ExifTag(0xA, "GPSMeasureMode", "GPS measurement mode"));
            Add(new ExifTag(0xC, "GPSSpeedRef", "Speed unit"));
            Add(new ExifTag(0xE, "GPSTrackRef", "Reference for direction of movement"));
            Add(new ExifTag(0x10, "GPSImgDirectionRef", "Reference for direction of image"));
            Add(new ExifTag(0x12, "GPSMapDatum", "Geodetic survey data used"));
            Add(new ExifTag(0x13, "GPSDestLatitudeRef", "Reference for latitude of destination"));
            Add(new ExifTag(0x15, "GPSDestLongitudeRef", "Reference for longitude of destination"));
            Add(new ExifTag(0x17, "GPSDestBearingRef", "Reference for bearing of destination"));
            Add(new ExifTag(0x19, "GPSDestDistanceRef", "Reference for distance to destination"));
            Add(new ExifTag(0x1D, "GPSDateStamp", "GPS date"));
            Add(new ExifTag(0x8828, "OECF", "Optoelectric conversion factor"));
            Add(new ExifTag(0xA20C, "SpatialFrequencyResponse", "Spatial frequency response"));
            Add(new ExifTag(0xA300, "FileSource", "File source"));
            Add(new ExifTag(0xA301, "SceneType", "Scene type"));
            Add(new ExifTag(0xA302, "CFAPattern", "CFA pattern"));
            Add(new ExifTag(0xA40B, "DeviceSettingDescription", "Device settings description"));
            Add(new ExifTag(0x9000, "ExifVersion", "Exif version"));
            Add(new ExifTag(0xA000, "FlashpixVersion", "Supported Flashpix version"));
            Add(new ExifTag(0x9101, "ComponentsConfiguration", "Meaning of each component"));
            Add(new ExifTag(0x927C, "MakerNote", "Manufacturer notes"));
            Add(new ExifTag(0x9286, "UserComment", "User comments"));
            Add(new ExifTag(0x1B, "GPSProcessingMethod", "Name of GPS processing method"));
            Add(new ExifTag(0x1C, "GPSAreaInformation", "Name of GPS area"));
        }

        public static SupportedTags GetInstance()
        {
            return _inst ?? (_inst = new SupportedTags());
        }
    }

    #endregion

    #region ExifTags

    public sealed class ExifTags : Dictionary<string, ExifTag>
    {
        #region Fields

        private static readonly List<ExifTag> _supportedTags = SupportedTags.GetInstance();
        private static readonly Encoding Ascii = Encoding.ASCII;

        #endregion

        #region Property

        public static IEnumerable<string> SupportedTagNames
        {
            get
            {
                var tags = from tag in _supportedTags
                           select tag.FieldName;
                return tags;
            }
        }

        #endregion

        #region Constructor

        public ExifTags(string fileName)
            : this(fileName, true, false)
        {
        }

        public ExifTags(Image image)
        {
            ReadTags(image);
        }

        public ExifTags(string fileName, bool useEmbeddedColorManagement, bool validateImageData)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var image = Image.FromStream(stream,
                    useEmbeddedColorManagement,
                    validateImageData);

                ReadTags(image);
            }
        }

        #endregion

        #region Public Method

        public static string ReadTag(Image image, ExifFileds exifFiledType)
        {
            return ReadTag(image, (int)exifFiledType);
        }

        #endregion

        #region Helper

        private static string ReadTag(Image image, int propId)
        {
            string value = string.Empty;
            PropertyItem pitem;

            try
            {
                pitem = image.GetPropertyItem(propId);
            }
            catch (Exception ex)
            {


                throw ex;
            }


            switch (pitem.Type)
            {
                case 0x1:
                    if (pitem.Value.Length == 4)
                        value = "Version " + pitem.Value[0].ToString() + "." + pitem.Value[1].ToString();
                    else if (pitem.Id == 0x5 && pitem.Value[0] == 0)
                        value = "Sea level";
                    else
                        value = pitem.Value[0].ToString();
                    break;
                case 0x2:
                    value = Ascii.GetString(pitem.Value).Trim('\0');
                    if (pitem.Id == 0x1 || pitem.Id == 0x13)
                        if (value == "N") value = "North latitude";
                        else if (value == "S") value = "South latitude";
                        else value = "reserved";
                    if (pitem.Id == 0x3 || pitem.Id == 0x15)
                        if (value == "E") value = "East longitude";
                        else if (value == "W") value = "West longitude";
                        else value = "reserved";
                    if (pitem.Id == 0x9)
                        if (value == "A") value = "Measurement in progress";
                        else if (value == "V") value = "Measurement Interoperability";
                        else value = "reserved";
                    if (pitem.Id == 0xA)
                        if (value == "2") value = "2-dimensional measurement";
                        else if (value == "3") value = "3-dimensional measurement";
                        else value = "reserved";
                    if (pitem.Id == 0xC || pitem.Id == 0x19)
                        if (value == "K") value = "Kilometers per hour";
                        else if (value == "M") value = "Miles per hour";
                        else if (value == "N") value = "Knots";
                        else value = "reserved";
                    if (pitem.Id == 0xE || pitem.Id == 0x10 || pitem.Id == 0x17)
                        if (value == "T") value = "True direction";
                        else if (value == "M") value = "Magnetic direction";
                        else value = "reserved";
                    break;
                case 0x3:
                    {
                        #region 3 = SHORT (16-bit unsigned int)

                        UInt16 uintval = BitConverter.ToUInt16(pitem.Value, 0);

                        // orientation // lookup table					
                        switch (pitem.Id)
                        {
                            case 0x8827: // ISO speed rating
                                value = "ISO-" + uintval.ToString();
                                break;
                            case 0xA217: // sensing method
                                {
                                    switch (uintval)
                                    {
                                        case 1: value = "Not defined"; break;
                                        case 2: value = "One-chip color area sensor"; break;
                                        case 3: value = "Two-chip color area sensor"; break;
                                        case 4: value = "Three-chip color area sensor"; break;
                                        case 5: value = "Color sequential area sensor"; break;
                                        case 7: value = "Trilinear sensor"; break;
                                        case 8: value = "Color sequential linear sensor"; break;
                                        default: value = " reserved"; break;
                                    }
                                }
                                break;
                            case 0x8822: // 曝光程序
                                switch (uintval)
                                {
                                    case 0: value = "未定义"; break;
                                    case 1: value = "手工"; break;
                                    case 2: value = "标准程序"; break;
                                    case 3: value = "光圈优先"; break;
                                    case 4: value = "快门优先"; break;
                                    case 5: value = "景深优先"; break;
                                    case 6: value = "运动模式"; break;
                                    case 7: value = "肖像模式"; break;
                                    case 8: value = "风景模式"; break;
                                    default: value = "保留"; break;
                                }
                                break;
                            case 0x9207: // 测光模式
                                switch (uintval)
                                {
                                    case 0: value = "未知"; break;
                                    case 1: value = "平均"; break;
                                    case 2: value = "中央重点平均"; break;
                                    case 3: value = "点测"; break;
                                    case 4: value = "多点"; break;
                                    case 5: value = "评估"; break;
                                    case 6: value = "局部"; break;
                                    case 255: value = "其他"; break;
                                    default: value = "保留"; break;
                                }
                                break;
                            case 0x9208: // Light source
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "unknown"; break;
                                        case 1: value = "Daylight"; break;
                                        case 2: value = "Fluorescent"; break;
                                        case 3: value = "Tungsten (incandescent light)"; break;
                                        case 4: value = "Flash"; break;
                                        case 9: value = "Fine weather"; break;
                                        case 10: value = "Cloudy weather"; break;
                                        case 11: value = "Shade"; break;
                                        case 12: value = "Daylight fluorescent (D 5700 – 7100K)"; break;
                                        case 13: value = "Day white fluorescent (N 4600 – 5400K)"; break;
                                        case 14: value = "Cool white fluorescent (W 3900 – 4500K)"; break;
                                        case 15: value = "White fluorescent (WW 3200 – 3700K)"; break;
                                        case 17: value = "Standard light A"; break;
                                        case 18: value = "Standard light B"; break;
                                        case 19: value = "Standard light C"; break;
                                        case 20: value = "D55"; break;
                                        case 21: value = "D65"; break;
                                        case 22: value = "D75"; break;
                                        case 23: value = "D50"; break;
                                        case 24: value = "ISO studio tungsten"; break;
                                        case 255: value = "ISO studio tungsten"; break;
                                        default: value = "other light source"; break;
                                    }
                                }
                                break;
                            case 0x9209: // Flash
                                {
                                    switch (uintval)
                                    {
                                        case 0x0: value = "关闭"; break;
                                        case 0x1: value = "Flash fired"; break;
                                        case 0x5: value = "Strobe return light not detected"; break;
                                        case 0x7: value = "Strobe return light detected"; break;
                                        case 0x9: value = "Flash fired, compulsory flash mode"; break;
                                        case 0xD: value = "Flash fired, compulsory flash mode, return light not detected"; break;
                                        case 0xF: value = "Flash fired, compulsory flash mode, return light detected"; break;
                                        case 0x10: value = "Flash did not fire, compulsory flash mode"; break;
                                        case 0x18: value = "Flash did not fire, auto mode"; break;
                                        case 0x19: value = "Flash fired, auto mode"; break;
                                        case 0x1D: value = "Flash fired, auto mode, return light not detected"; break;
                                        case 0x1F: value = "Flash fired, auto mode, return light detected"; break;
                                        case 0x20: value = "No flash function"; break;
                                        case 0x41: value = "Flash fired, red-eye reduction mode"; break;
                                        case 0x45: value = "Flash fired, red-eye reduction mode, return light not detected"; break;
                                        case 0x47: value = "Flash fired, red-eye reduction mode, return light detected"; break;
                                        case 0x49: value = "Flash fired, compulsory flash mode, red-eye reduction mode"; break;
                                        case 0x4D: value = "Flash fired, compulsory flash mode, red-eye reduction mode, return light not detected"; break;
                                        case 0x4F: value = "Flash fired, compulsory flash mode, red-eye reduction mode, return light detected"; break;
                                        case 0x59: value = "Flash fired, auto mode, red-eye reduction mode"; break;
                                        case 0x5D: value = "Flash fired, auto mode, return light not detected, red-eye reduction mode"; break;
                                        case 0x5F: value = "Flash fired, auto mode, return light detected, red-eye reduction mode"; break;
                                        default: value = "reserved"; break;
                                    }
                                }
                                break;
                            case 0x0128: //ResolutionUnit
                                {
                                    switch (uintval)
                                    {
                                        case 2: value = "Inch"; break;
                                        case 3: value = "Centimeter"; break;
                                        default: value = "No Unit"; break;
                                    }
                                }
                                break;
                            case 0xA409: // Saturation
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Normal"; break;
                                        case 1: value = "Low saturation"; break;
                                        case 2: value = "High saturation"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;

                            case 0xA40A: // Sharpness
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Normal"; break;
                                        case 1: value = "Soft"; break;
                                        case 2: value = "Hard"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA408: // Contrast
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Normal"; break;
                                        case 1: value = "Soft"; break;
                                        case 2: value = "Hard"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0x103: // Compression
                                {
                                    switch (uintval)
                                    {
                                        case 1: value = "Uncompressed"; break;
                                        case 6: value = "JPEG compression (thumbnails only)"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0x106: // PhotometricInterpretation
                                {
                                    switch (uintval)
                                    {
                                        case 2: value = "RGB"; break;
                                        case 6: value = "YCbCr"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0x112: // Orientation
                                {
                                    switch (uintval)
                                    {
                                        case 1: value = "The 0th row is at the visual top of the image, and the 0th column is the visual left-hand side."; break;
                                        case 2: value = "The 0th row is at the visual top of the image, and the 0th column is the visual right-hand side."; break;
                                        case 3: value = "The 0th row is at the visual bottom of the image, and the 0th column is the visual right-hand side."; break;
                                        case 4: value = "The 0th row is at the visual bottom of the image, and the 0th column is the visual left-hand side."; break;
                                        case 5: value = "The 0th row is the visual left-hand side of the image, and the 0th column is the visual top."; break;
                                        case 6: value = "The 0th row is the visual right-hand side of the image, and the 0th column is the visual top."; break;
                                        case 7: value = "The 0th row is the visual right-hand side of the image, and the 0th column is the visual bottom."; break;
                                        case 8: value = "The 0th row is the visual left-hand side of the image, and the 0th column is the visual bottom."; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0x213: // YCbCrPositioning
                                {
                                    switch (uintval)
                                    {
                                        case 1: value = "centered"; break;
                                        case 6: value = "co-sited"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA001: // ColorSpace
                                {
                                    switch (uintval)
                                    {
                                        case 1: value = "sRGB"; break;
                                        case 0xFFFF: value = "Uncalibrated"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA401: // CustomRendered
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Normal process"; break;
                                        case 1: value = "Custom process"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA402: // ExposureMode
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Auto exposure"; break;
                                        case 1: value = "Manual exposure"; break;
                                        case 2: value = "Auto bracket"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA403: // WhiteBalance
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Auto white balance"; break;
                                        case 1: value = "Manual white balance"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA406: // SceneCaptureType
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Standard"; break;
                                        case 1: value = "Landscape"; break;
                                        case 2: value = "Portrait"; break;
                                        case 3: value = "Night scene"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;

                            case 0xA40C: // SubjectDistanceRange
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "unknown"; break;
                                        case 1: value = "Macro"; break;
                                        case 2: value = "Close view"; break;
                                        case 3: value = "Distant view"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0x1E: // GPSDifferential
                                {
                                    switch (uintval)
                                    {
                                        case 0: value = "Measurement without differential correction"; break;
                                        case 1: value = "Differential correction applied"; break;
                                        default: value = "Reserved"; break;
                                    }
                                }
                                break;
                            case 0xA405: // FocalLengthIn35mmFilm
                                value = uintval + " mm";
                                break;
                            default://
                                value = uintval.ToString();
                                break;
                        }
                        #endregion
                    }
                    break;
                case 0x4:
                    value = BitConverter.ToUInt32(pitem.Value, 0).ToString();
                    break;
                case 0x5:
                    {
                        #region 5 = RATIONAL (Two LONGs, unsigned)

                        var rat = new URational(pitem.Value);

                        switch (pitem.Id)
                        {
                            case 0x9202: // ApertureValue
                                value = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), rat.ToDouble()), 2).ToString();
                                break;
                            case 0x9205: // MaxApertureValue
                                value = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), rat.ToDouble()), 2).ToString();
                                break;
                            case 0x920A: // FocalLength
                                value = rat.ToDouble().ToString() + " mm";
                                break;
                            case 0x829D: // F-number
                                value = "F/" + rat.ToDouble().ToString();
                                break;
                            case 0x11A: // Xresolution
                                value = rat.ToDouble().ToString();
                                break;
                            case 0x11B: // Yresolution
                                value = rat.ToDouble().ToString();
                                break;
                            case 0x829A: // ExposureTime
                                value = rat.ToString() + " sec";
                                break;
                            case 0x2: // GPSLatitude                                
                                value = new GPSRational(pitem.Value).ToString();
                                break;
                            case 0x4: // GPSLongitude
                                value = new GPSRational(pitem.Value).ToString();
                                break;
                            case 0x6: // GPSAltitude
                                value = rat.ToDouble() + " meters";
                                break;
                            case 0xA404: // Digital Zoom Ratio
                                value = rat.ToDouble().ToString();
                                if (value == "0") value = "none";
                                break;
                            case 0xB: // GPSDOP
                                value = rat.ToDouble().ToString();
                                break;
                            case 0xD: // GPSSpeed
                                value = rat.ToDouble().ToString();
                                break;
                            case 0xF: // GPSTrack
                                value = rat.ToDouble().ToString();
                                break;
                            case 0x11: // GPSImgDir
                                value = rat.ToDouble().ToString();
                                break;
                            case 0x14: // GPSDestLatitude
                                value = new GPSRational(pitem.Value).ToString();
                                break;
                            case 0x16: // GPSDestLongitude
                                value = new GPSRational(pitem.Value).ToString();
                                break;
                            case 0x18: // GPSDestBearing
                                value = rat.ToDouble().ToString();
                                break;
                            case 0x1A: // GPSDestDistance
                                value = rat.ToDouble().ToString();
                                break;
                            case 0x7: // GPSTimeStamp                                
                                value = new GPSRational(pitem.Value).ToString(":");
                                break;

                            default:
                                value = rat.ToString();
                                break;
                        }

                        #endregion
                    }
                    break;
                case 0x7:
                    switch (pitem.Id)
                    {
                        case 0xA300: //FileSource
                            {
                                value = pitem.Value[0] == 3 ? "DSC" : "reserved";
                                break;
                            }
                        case 0xA301: //SceneType
                            value = pitem.Value[0] == 1 ? "A directly photographed image" : "reserved";
                            break;
                        case 0x9000:// Exif Version
                            value = Ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0xA000: // Flashpix Version
                            value = Ascii.GetString(pitem.Value).Trim('\0');
                            value = value == "0100" ? "Flashpix Format Version 1.0" : "reserved";
                            break;
                        case 0x9101: //ComponentsConfiguration
                            value = GetComponentsConfig(pitem.Value);
                            break;
                        case 0x927C: //MakerNote
                            value = Ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0x9286: //UserComment
                            value = Ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0x1B: //GPS Processing Method
                            value = Ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0x1C: //GPS Area Info
                            value = Ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        default:
                            value = "-";
                            break;
                    }
                    break;
                case 0x9:
                    value = BitConverter.ToInt32(pitem.Value, 0).ToString();
                    break;
                case 0xA:
                    {
                        #region 10 = SRATIONAL (Two SLONGs, signed)

                        var rat = new Rational(pitem.Value);

                        switch (pitem.Id)
                        {
                            case 0x9201: // ShutterSpeedValue
                                value = "1/" + Math.Round(Math.Pow(2, rat.ToDouble()), 2).ToString();
                                break;
                            case 0x9203: // BrightnessValue
                                value = Math.Round(rat.ToDouble(), 4).ToString();
                                break;
                            case 0x9204: // ExposureBiasValue
                                value = Math.Round(rat.ToDouble(), 2).ToString() + " eV";
                                break;
                            default:
                                value = rat.ToString();
                                break;
                        }
                        #endregion
                    }
                    break;
            }

            return value;
        }

        private void ReadTags(Image image)
        {
            var tagsInFile = from tag in _supportedTags
                             join id in image.PropertyIdList on tag.Id equals id
                             select tag;

            foreach (var tag in tagsInFile)
            {
                tag.Value = ReadTag(image, tag.Id);
                Add(tag.FieldName, tag);
            }
        }

        private static string GetComponentsConfig(byte[] bytes)
        {
            var s = string.Empty;
            var vals = new[] { "", "Y", "Cb", "Cr", "R", "G", "B" };

            foreach (var b in bytes)
                s += vals[b];

            return s;
        }

        #endregion


    }

    #endregion

    /// <summary>
    /// Exif 枚举
    /// </summary>
    public enum ExifFileds
    {
        /// <summary>
        /// Image width
        /// </summary>
        ImageWidth = 0x100,

        /// <summary>
        /// Image height
        /// </summary>
        ImageHeight = 0x101,

        /// <summary>
        /// GPS tag version
        /// </summary>
        GPSVersionID = 0x0,

        /// <summary>
        /// Altitude reference
        /// </summary>
        GPSAltitudeRef = 0x5,

        /// <summary>
        /// Image data location
        /// </summary>
        StripOffsets = 0x111,

        /// <summary>
        /// Number of rows per strip
        /// </summary>
        RowsPerStrip = 0x116,

        /// <summary>
        /// Bytes per compressed strip
        /// </summary>
        StripByteCounts = 0x117,

        /// <summary>
        /// Valid image height
        /// </summary>
        PixelXDimension = 0xA003,

        /// <summary>
        /// Number of bits per component
        /// </summary>
        BitsPerSample = 0x102,

        /// <summary>
        /// Compression scheme
        /// </summary>
        Compression = 0x103,

        /// <summary>
        /// Pixel composition
        /// </summary>
        PhotometricInterpretation = 0x106,

        /// <summary>
        /// Orientation of image
        /// </summary>
        Orientation = 0x112,

        /// <summary>
        /// Number of components
        /// </summary>
        SamplesPerPixel = 0x115,

        /// <summary>
        /// Image data arrangement
        /// </summary>
        PlanarConfiguration = 0x11C,

        /// <summary>
        /// Subsampling ratio of Y to C
        /// </summary>
        YCbCrSubSampling = 0x212,

        /// <summary>
        /// Y and C positioning
        /// </summary>
        YCbCrPositioning = 0x213,

        /// <summary>
        /// Unit of X and Y resolution
        /// </summary>
        ResolutionUnit = 0x128,

        /// <summary>
        /// Transfer function
        /// </summary>
        TransferFunction = 0x12D,

        /// <summary>
        /// Color space information
        /// </summary>
        ColorSpace = 0xA001,

        /// <summary>
        /// Exposure program
        /// </summary>
        ExposureProgram = 0x8822,

        /// <summary>
        /// ISO speed rating
        /// </summary>
        ISOSpeedRatings = 0x8827,

        /// <summary>
        /// Metering mode
        /// </summary>
        MeteringMode = 0x9207,

        /// <summary>
        /// Light source
        /// </summary>
        LightSource = 0x9208,

        /// <summary>
        /// Flash
        /// </summary>
        Flash = 0x9209,

        /// <summary>
        /// Subject area
        /// </summary>
        SubjectArea = 0x9214,

        /// <summary>
        /// Focal plane resolution unit
        /// </summary>
        FocalPlaneResolutionUnit = 0xA210,

        /// <summary>
        /// Subject location
        /// </summary>
        SubjectLocation = 0xA214,

        /// <summary>
        /// Sensing method
        /// </summary>
        SensingMethod = 0xA217,

        /// <summary>
        /// Custom image processing
        /// </summary>
        CustomRendered = 0xA401,

        /// <summary>
        /// Exposure mode
        /// </summary>
        ExposureMode = 0xA402,

        /// <summary>
        /// White balance
        /// </summary>
        WhiteBalance = 0xA403,

        /// <summary>
        /// Focal length in 35 mm film
        /// </summary>
        FocalLengthIn35mmFilm = 0xA405,

        /// <summary>
        /// Scene capture type
        /// </summary>
        SceneCaptureType = 0xA406,

        /// <summary>
        /// Contrast
        /// </summary>
        Contrast = 0xA408,

        /// <summary>
        /// Saturation
        /// </summary>
        Saturation = 0xA409,

        /// <summary>
        /// Sharpness
        /// </summary>
        Sharpness = 0xA40A,

        /// <summary>
        /// Subject distance range
        /// </summary>
        SubjectDistanceRange = 0xA40C,

        /// <summary>
        /// GPS differential correction
        /// </summary>
        GPSDifferential = 0x1E,

        /// <summary>
        /// Shutter speed
        /// </summary>
        ShutterSpeedValue = 0x9201,

        /// <summary>
        /// Brightness
        /// </summary>
        BrightnessValue = 0x9203,

        /// <summary>
        /// Exposure bias
        /// </summary>
        ExposureBiasValue = 0x9204,

        /// <summary>
        /// Offset to JPEG SOI
        /// </summary>
        JPEGInterchangeFormat = 0x201,

        /// <summary>
        /// Bytes of JPEG data
        /// </summary>
        JPEGInterchangeFormatLength = 0x202,

        /// <summary>
        /// Image resolution in width direction
        /// </summary>
        XResolution = 0x11A,

        /// <summary>
        /// Image resolution in height direction
        /// </summary>
        YResolution = 0x11B,

        /// <summary>
        /// White point chromaticity
        /// </summary>
        WhitePoint = 0x13E,

        /// <summary>
        /// Chromaticities of primaries
        /// </summary>
        PrimaryChromaticities = 0x13F,

        /// <summary>
        /// Color space transformation matrix coefficients
        /// </summary>
        YCbCrCoefficients = 0x211,

        /// <summary>
        /// Pair of black and white reference values
        /// </summary>
        ReferenceBlackWhite = 0x214,

        /// <summary>
        /// Image compression mode
        /// </summary>
        CompressedBitsPerPixel = 0x9102,

        /// <summary>
        /// Exposure time
        /// </summary>
        ExposureTime = 0x829A,

        /// <summary>
        /// F number
        /// </summary>
        FNumber = 0x829D,

        /// <summary>
        /// Aperture
        /// </summary>
        ApertureValue = 0x9202,

        /// <summary>
        /// Maximum lens aperture
        /// </summary>
        MaxApertureValue = 0x9205,

        /// <summary>
        /// Subject distance
        /// </summary>
        SubjectDistance = 0x9206,

        /// <summary>
        /// Lens focal length
        /// </summary>
        FocalLength = 0x920A,

        /// <summary>
        /// Flash energy
        /// </summary>
        FlashEnergy = 0xA20B,

        /// <summary>
        /// Focal plane X resolution
        /// </summary>
        FocalPlaneXResolution = 0xA20E,

        /// <summary>
        /// Focal plane Y resolution
        /// </summary>
        FocalPlaneYResolution = 0xA20F,

        /// <summary>
        /// Exposure index
        /// </summary>
        ExposureIndex = 0xA215,

        /// <summary>
        /// Digital zoom ratio
        /// </summary>
        DigitalZoomRatio = 0xA404,

        /// <summary>
        /// Gain control
        /// </summary>
        GainControl = 0xA407,

        /// <summary>
        /// Latitude
        /// </summary>
        GPSLatitude = 0x2,

        /// <summary>
        /// Longitude
        /// </summary>
        GPSLongitude = 0x4,

        /// <summary>
        /// Altitude
        /// </summary>
        GPSAltitude = 0x6,

        /// <summary>
        /// GPS time (atomic clock)
        /// </summary>
        GPSTimeStamp = 0x7,

        /// <summary>
        /// Measurement precision
        /// </summary>
        GPSDOP = 0xB,

        /// <summary>
        /// Speed of GPS receiver
        /// </summary>
        GPSSpeed = 0xD,

        /// <summary>
        /// Direction of movement
        /// </summary>
        GPSTrack = 0xF,

        /// <summary>
        /// Direction of image
        /// </summary>
        GPSImgDirection = 0x11,

        /// <summary>
        /// Latitude of destination
        /// </summary>
        GPSDestLatitude = 0x14,

        /// <summary>
        /// Longitude of destination
        /// </summary>
        GPSDestLongitude = 0x16,

        /// <summary>
        /// Bearing of destination
        /// </summary>
        GPSDestBearing = 0x18,

        /// <summary>
        /// Distance to destination
        /// </summary>
        GPSDestDistance = 0x1A,

        /// <summary>
        /// File change date and time
        /// </summary>
        DateTime = 0x132,

        /// <summary>
        /// Image title
        /// </summary>
        ImageDescription = 0x10E,

        /// <summary>
        /// Image input equipment manufacturer
        /// </summary>
        Make = 0x10F,

        /// <summary>
        /// Image input equipment model
        /// </summary>
        Model = 0x110,

        /// <summary>
        /// Software used
        /// </summary>
        Software = 0x131,

        /// <summary>
        /// Person who created the image
        /// </summary>
        Artist = 0x13B,

        /// <summary>
        /// Copyright holder
        /// </summary>
        Copyright = 0x8298,

        /// <summary>
        /// Related audio file
        /// </summary>
        RelatedSoundFile = 0xA004,

        /// <summary>
        /// Date and time of original data generation
        /// </summary>
        DateTimeOriginal = 0x9003,

        /// <summary>
        /// /Date and time of digital data generation
        /// </summary>
        DateTimeDigitized = 0x9004,

        /// <summary>
        /// DateTime subseconds
        /// </summary>
        SubSecTime = 0x9290,

        /// <summary>
        /// DateTimeOriginal subseconds
        /// </summary>
        SubSecTimeOriginal = 0x9291,

        /// <summary>
        /// DateTimeDigitized subseconds
        /// </summary>
        SubSecTimeDigitized = 0x9292,

        /// <summary>
        /// Unique image ID
        /// </summary>
        ImageUniqueID = 0xA420,

        /// <summary>
        /// Spectral sensitivity
        /// </summary>
        SpectralSensitivity = 0x8824,

        /// <summary>
        /// North or South Latitude
        /// </summary>
        GPSLatitudeRef = 0x1,

        /// <summary>
        /// East or West Longitude
        /// </summary>
        GPSLongitudeRef = 0x3,

        /// <summary>
        /// GPS satellites used for measurement
        /// </summary>
        GPSSatellites = 0x8,

        /// <summary>
        /// GPS receiver status
        /// </summary>
        GPSStatus = 0x9,

        /// <summary>
        /// GPS measurement mode
        /// </summary>
        GPSMeasureMode = 0xA,

        /// <summary>
        /// Speed unit
        /// </summary>
        GPSSpeedRef = 0xC,

        /// <summary>
        /// Reference for direction of movement
        /// </summary>
        GPSTrackRef = 0xE,

        /// <summary>
        /// Reference for direction of image
        /// </summary>
        GPSImgDirectionRef = 0x10,

        /// <summary>
        /// Geodetic survey data used
        /// </summary>
        GPSMapDatum = 0x12,

        /// <summary>
        /// Reference for latitude of destination
        /// </summary>
        GPSDestLatitudeRef = 0x13,

        /// <summary>
        /// Reference for longitude of destination
        /// </summary>
        GPSDestLongitudeRef = 0x15,

        /// <summary>
        /// Reference for bearing of destination
        /// </summary>
        GPSDestBearingRef = 0x17,

        /// <summary>
        /// Reference for distance to destination
        /// </summary>
        GPSDestDistanceRef = 0x19,

        /// <summary>
        /// "GPS date
        /// </summary>
        GPSDateStamp = 0x1D,

        /// <summary>
        /// Optoelectric conversion factor
        /// </summary>
        OECF = 0x8828,

        /// <summary>
        /// Spatial frequency response
        /// </summary>
        SpatialFrequencyResponse = 0xA20C,

        /// <summary>
        /// File source
        /// </summary>
        FileSource = 0xA300,

        /// <summary>
        /// Scene type"
        /// </summary>
        SceneType = 0xA301,

        /// <summary>
        /// CFA pattern
        /// </summary>
        CFAPattern = 0xA302,

        /// <summary>
        /// Device settings description
        /// </summary>
        DeviceSettingDescription = 0xA40B,

        /// <summary>
        /// Exif version
        /// </summary>
        ExifVersion = 0x9000,

        /// <summary>
        /// Supported Flashpix version
        /// </summary>
        FlashpixVersion = 0xA000,

        /// <summary>
        /// Meaning of each component
        /// </summary>
        ComponentsConfiguration = 0x9101,

        /// <summary>
        /// Manufacturer notes
        /// </summary>
        MakerNote = 0x927C,

        /// <summary>
        /// User comments
        /// </summary>
        UserComment = 0x9286,

        /// <summary>
        /// Name of GPS processing method
        /// </summary>
        GPSProcessingMethod = 0x1B,

        /// <summary>
        /// Name of GPS area
        /// </summary>
        GPSAreaInformation = 0x1C
    }

    internal sealed class Rational
    {
        private readonly Int32 _num;
        private readonly Int32 _denom;

        public Rational(byte[] bytes)
        {
            byte[] n = new byte[4];
            byte[] d = new byte[4];
            Array.Copy(bytes, 0, n, 0, 4);
            Array.Copy(bytes, 4, d, 0, 4);
            _num = BitConverter.ToInt32(n, 0);
            _denom = BitConverter.ToInt32(d, 0);
        }

        public double ToDouble()
        {
            return Math.Round(Convert.ToDouble(_num) / Convert.ToDouble(_denom), 2);
        }

        public string ToString(string separator)
        {
            return _num.ToString() + separator + _denom.ToString();
        }

        public override string ToString()
        {
            return ToString("/");
        }
    }

    internal sealed class URational
    {
        private readonly UInt32 _num;
        private readonly UInt32 _denom;

        public URational(byte[] bytes)
        {
            byte[] n = new byte[4];
            byte[] d = new byte[4];
            Array.Copy(bytes, 0, n, 0, 4);
            Array.Copy(bytes, 4, d, 0, 4);
            _num = BitConverter.ToUInt32(n, 0);
            _denom = BitConverter.ToUInt32(d, 0);
        }

        public double ToDouble()
        {
            return Math.Round(Convert.ToDouble(_num) / Convert.ToDouble(_denom), 2);
        }

        public override string ToString()
        {
            return ToString("/");
        }

        public string ToString(string separator)
        {
            return _num.ToString() + separator + _denom.ToString();
        }
    }

    internal sealed class GPSRational
    {
        private Rational _hours;
        private Rational _minutes;
        private Rational _seconds;

        public Rational Hours
        {
            get
            {
                return _hours;
            }
            set
            {
                _hours = value;
            }
        }
        public Rational Minutes
        {
            get
            {
                return _minutes;
            }
            set
            {
                _minutes = value;
            }
        }
        public Rational Seconds
        {
            get
            {
                return _seconds;
            }
            set
            {
                _seconds = value;
            }
        }

        public GPSRational(byte[] bytes)
        {
            byte[] h = new byte[8]; byte[] m = new byte[8]; byte[] s = new byte[8];

            Array.Copy(bytes, 0, h, 0, 8); Array.Copy(bytes, 8, m, 0, 8); Array.Copy(bytes, 16, s, 0, 8);

            _hours = new Rational(h);
            _minutes = new Rational(m);
            _seconds = new Rational(s);
        }

        public override string ToString()
        {
            return _hours.ToDouble() + "° "
                + _minutes.ToDouble() + "\' "
                + _seconds.ToDouble() + "\"";
        }

        public string ToString(string separator)
        {
            return _hours.ToDouble() + separator
                + _minutes.ToDouble() + separator +
                _seconds.ToDouble();
        }
    }

    #endregion
}
