using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Primitives;
using System.Xml;

namespace Bussines {
    public class Entrance {
        public DateTime EntranceDateTime { get; set; }
        public Equip Equip { get; set; }
    }
    public class Solution {
        public string Description { get; set; }
        public double Cost { get; set; }
        public DateTime SolutionDateTime { get; set; }
    }
    public class Equip {
        public string Name { get; set; }
        public string Client { get; set; }
        public string Problem { get; set; }
        public Solution Solution { get; set; }
    }

    public static class SecureNotePageFile {
        public static Settings Load(string fileName) {
            byte _protected = 0;
            int _itemsCount = 0;
            long _lastModified = 0;
            string _displayName = "";
            ObservableCollection<Entrance> _notes;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                using (BinaryReader br = new BinaryReader(fs, UTF7Encoding.UTF7)) {
                    byte[] header = new byte[] { 0xa7, 0x6e, 0x70, 0xb1, 0x53, 0x74, 0x54, 0x2e, 0x53, 0x4e, 0x50 };
                    if (Array.Equals(br.ReadBytes(header.Length), header))
                        throw new FileLoadException();
                    _protected = br.ReadByte();
                    _itemsCount = br.ReadInt16();
                    _displayName = br.ReadString();
                    _lastModified = br.ReadInt64();

                    _notes = new ObservableCollection<Entrance>();
                    br.BaseStream.Position = 150;
                    byte[] npHeader = new byte[] { 0x4e, 0x50 };
                    for (int i = 0; i < _itemsCount; i++) {
                        if (Array.Equals(br.ReadBytes(npHeader.Length), npHeader))
                            throw new InvalidDataException();
                        int _textLength = br.ReadInt32();
                        long _creationDate = br.ReadInt64();
                        string _text = Encoding.UTF7.GetString(br.ReadBytes(_textLength));
                        _notes.Add(new Entrance() {
                            Equip = FromXML(_text),
                            EntranceDateTime = DateTime.FromBinary(_creationDate),
                        });
                    }
                    br.Close();
                }
                Settings np = new Settings() {
                    DisplayName = _displayName,
                    IsProtected = _protected != 0,
                    Entrances = _notes
                };
                fs.Close();
                return np;
            }
        }

        private static Equip FromXML(string _text) {
            Equip equip = new Equip();
            XmlDocument xmld = new XmlDocument();
            xmld.LoadXml(_text);
            equip.Client = xmld.ChildNodes[0].Attributes[0].Value;
            equip.Name = xmld.ChildNodes[0].Attributes[1].Value;
            equip.Problem = xmld.ChildNodes[0].ChildNodes[0].InnerText;
            if (xmld.ChildNodes[0].ChildNodes[1].HasChildNodes)
                equip.Solution = new Solution() {
                    Cost = Convert.ToDouble(xmld.ChildNodes[0].ChildNodes[1].FirstChild.Attributes[0].Value),
                    Description = xmld.ChildNodes[0].ChildNodes[1].FirstChild.Attributes[1].Value,
                    SolutionDateTime = DateTime.FromBinary(Convert.ToInt64(xmld.ChildNodes[0].ChildNodes[1].FirstChild.Attributes[2].Value)),
                };
            return equip;
        }

        public static void Save(string fileName, Settings np) {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)) {
                using (BinaryWriter bw = new BinaryWriter(fs, UTF7Encoding.UTF7)) {
                    byte[] header = new byte[] { 0xa7, 0x6e, 0x70, 0xb1, 0x53, 0x74, 0x54, 0x2e, 0x53, 0x4e, 0x50 };
                    bw.Write(header);//Header 11 bytes
                    bw.Write(np.IsProtected);//IsProtected 1 byte
                    bw.Write((UInt16)np.Entrances.Count);//NotesCount 2 byte
                    bw.Write(np.DisplayName != null ? np.DisplayName : "");//DisplayName max 100 byte
                    bw.Write(DateTime.Now.ToBinary());//LastDateModified 8 byte

                    bw.Seek(0x96, SeekOrigin.Begin);
                    byte[] npHeader = new byte[] { 0x4e, 0x50 };
                    foreach (Entrance i in np.Entrances) {//Notes
                        bw.Write(npHeader);
                        byte[] utfText = Encoding.UTF7.GetBytes(GetXML(i.Equip));
                        bw.Write(utfText.Length);
                        bw.Write(i.EntranceDateTime.ToBinary());
                        bw.Write(utfText);
                    }
                    bw.Close();
                }
                fs.Close();
            }
        }

        private static string GetXML(Equip equip) {
            string xml = string.Format(
                "<Equip Client=\"{0}\" Name=\"{1}\">\n\t<Equip.Problem>{2}</Equip.Problem>\n\t<Equip.Solution>\n\t{3}\n\t</Equip.Solution>\n</Equip>",
                equip.Client, equip.Name, equip.Problem,
                equip.Solution != null ? string.Format(
                "<Solution Cost=\"{0}\" Description=\"{1}\" SolutionDateTime=\"{2}\"/>",
                equip.Solution.Cost, equip.Solution.Description, equip.Solution.SolutionDateTime.ToBinary()) : "");
            return xml;
        }
    }

    public class Settings {
        bool _nullExtension = false;

        private static Settings instance = null;
        private static readonly object padlock = new object();

        public Settings() { }

        public static Settings Instance {
            get {
                if (instance == null) {
                    lock (padlock) {
                        if (instance == null) {
                            instance = new Settings();
                            instance.Entrances = new ObservableCollection<Entrance>();
                            instance.Output = new Dictionary<DateTime, DateTime>();
                        }
                    }
                }
                return instance;
            }
        }

        public void Initalise(string path) {
            if (File.Exists(path))
                Entrances = SecureNotePageFile.Load(path).Entrances;
            //try {
            //    Entrances = new ObservableCollection<Entrance>();
            //    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read)) {
            //        XmlDocument xmldoc = new XmlDocument();
            //        xmldoc.Load(fs);
            //        var xml = new XmlXamlReader();
            //        xml.AddKnowedTypes(typeof(Entrance));
            //        xml.AddKnowedTypes(typeof(Equip));
            //        xml.AddKnowedTypes(typeof(Solution));
            //        xml.Load(Instance, xmldoc);
            //    }
            //} catch { }
        }

        public void Save(string path) {
            //Xml(path, this);
            SecureNotePageFile.Save(path, this);
        }

        void Xml(string path, object obj) {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xmlws = new XmlWriterSettings();
            xmlws.ConformanceLevel = ConformanceLevel.Auto;
            xmlws.Indent = true;
            xmlws.OmitXmlDeclaration = true;
            XmlWriter xml = XmlWriter.Create(sb, xmlws);
            XmlWrite(xml, Settings.Instance, true);
            xml.Close();
            string xmlcode = sb.ToString();
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                byte[] xmlbyte = UTF8Encoding.UTF8.GetBytes(xmlcode);
                fs.Write(xmlbyte, 0, xmlbyte.Length);
                fs.Flush();
                fs.Close();
            }
        }

        private void XmlWrite(XmlWriter xmlw, object obj, bool isRoot) {
            MarkupObject markupObj = MarkupWriter.GetMarkupObjectFor(obj);
            Type objectType = markupObj.ObjectType;

            if (isRoot) {
                string nspace = objectType.Namespace;
                xmlw.WriteStartElement(objectType.Name, nspace);
            }

            List<MarkupProperty> propertyElements = new List<MarkupProperty>();
            foreach (System.Windows.Markup.Primitives.MarkupProperty markupProp in markupObj.Properties) {
                try {
                    if (!markupProp.IsComposite) {
                        string temp = markupProp.StringValue;
                        if (!string.IsNullOrEmpty(temp))
                            xmlw.WriteAttributeString(markupProp.Name, temp);
                    } else if (markupProp.Value.GetType() == typeof(System.Windows.Markup.NullExtension)) {
                        if (_nullExtension) {
                            xmlw.WriteAttributeString(markupProp.Name, "{x:Null}");
                        }
                    } else {
                        propertyElements.Add(markupProp);
                        //XmlWrite(xmlw, markupProp.Value, false);
                    }
                } catch { }
            }

            if (propertyElements.Count > 0) {
                foreach (MarkupProperty markupProp in propertyElements) {
                    string propElementName = markupObj.ObjectType.Name + "." + markupProp.Name;
                    if (isRoot) xmlw.WriteStartElement(propElementName);

                    System.Collections.IList collection = markupProp.Value as System.Collections.IList;
                    System.Collections.IDictionary dictionary = markupProp.Value as System.Collections.IDictionary;
                    if (collection != null && collection.Count > 0) {
                        foreach (object iobj in collection) {
                            XmlWrite(xmlw, iobj, true);
                            xmlw.WriteEndElement();
                        }
                    } else if (dictionary != null && dictionary.Count > 0) {
                        foreach (object iobj in dictionary) {
                            XmlWrite(xmlw, iobj, true);
                            xmlw.WriteEndElement();
                        }
                    } else {
                        if (markupProp.Value.GetType() == typeof(Equip) || markupProp.Value.GetType() == typeof(Solution))
                            XmlWrite(xmlw, markupProp.Value, true);
                        xmlw.WriteEndElement();
                    }
                }
            }
        }

        public ObservableCollection<Entrance> Entrances { get; set; }
        public Dictionary<DateTime, DateTime> Output { get; set; }

        public bool IsProtected { get; set; }

        public string DisplayName { get; set; }
    }
}
