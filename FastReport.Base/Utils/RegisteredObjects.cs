using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using FastReport.Data;
using FastReport.Export;

namespace FastReport.Utils
{
    /// <summary>
    /// Holds the information about the registered object.
    /// </summary>
    public partial class ObjectInfo
    {
        #region Fields
        private string name;
        private Object fObject;
        private string text;
        private int flags;
        private bool multiInsert;
        private bool enabled;
        private List<ObjectInfo> items;
        #endregion

        #region Properties
        /// <summary>
        /// Name of object or category.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The registered object.
        /// </summary>
        public Type Object
        {
            get { return fObject as Type; }
            set { fObject = value; }
        }

        /// <summary>
        /// The registered function.
        /// </summary>
        public MethodInfo Function
        {
            get { return fObject as MethodInfo; }
            set { fObject = value; }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (text == "")
                {
                    if (Object != null)
                        text = "Objects," + Object.Name;
                }
            }
        }

        /// <summary>
        /// Flags that will be used to create an object instance in the designer.
        /// </summary>
        public int Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        /// <summary>
        /// Indicates whether this object can be inserted several times simultaneously.
        /// </summary>
        /// <remarks>
        /// This is applied to Line object only.
        /// </remarks>
        public bool MultiInsert
        {
            get { return multiInsert; }
            set { multiInsert = value; }
        }

        /// <summary>
        /// Gets or sets the enabled flag for the object.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// List of subitems.
        /// </summary>
        public List<ObjectInfo> Items
        {
            get { return items; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enumerates all objects.
        /// </summary>
        /// <param name="list">List that will contain enumerated items.</param>
        public void EnumItems(List<ObjectInfo> list)
        {
            list.Add(this);
            foreach (ObjectInfo item in items)
            {
                item.EnumItems(list);
            }
        }

        internal ObjectInfo FindOrCreate(string complexName)
        {
            string[] itemNames = complexName.Split(new char[] { ',' });
            ObjectInfo root = this;
            foreach (string itemName in itemNames)
            {
                ObjectInfo item = null;
                for (int i = 0; i < root.Items.Count; i++)
                {
                    if (root.Items[i].Name != "" && root.Items[i].Name == itemName)
                    {
                        item = root.Items[i];
                        break;
                    }
                }
                if (item == null)
                {
                    item = new ObjectInfo();
                    item.Name = itemName;
                    item.Text = itemName;
                    root.Items.Add(item);
                }
                root = item;
            }
            return root;
        }

        internal void Update(Object obj, Bitmap image, int imageIndex, string text, int flags, bool multiInsert)
        {
            fObject = obj;
            UpdateDesign(obj, image, imageIndex, text, flags, multiInsert);
            Text = text;
            Flags = flags;
            MultiInsert = multiInsert;
        }

        internal void Update(Object obj, Bitmap image, int imageIndex, int buttonIndex, string text, int flags,
            bool multiInsert)
        {
            fObject = obj;
            UpdateDesign(obj, image, imageIndex, buttonIndex, text, flags, multiInsert);
            Text = text;
            Flags = flags;
            MultiInsert = multiInsert;
        }

        #endregion

        internal ObjectInfo()
        {
            items = new List<ObjectInfo>();
            name = "";
            enabled = true;
        }

        internal ObjectInfo(string name, Object obj, Bitmap image, int imageIndex, string text,
          int flags, bool multiInsert) : this()
        {
            this.name = name;
            Update(obj, image, imageIndex, text, flags, multiInsert);
        }
    }

    /// <summary>
    /// Contains all registered report items such as objects, export filters, wizards.
    /// </summary>
    /// <remarks>
    /// Use this class to register own components, wizards, export filters or another items that 
    /// need to be serialized to/from a report file.
    /// </remarks>
    /// <example>
    /// <code>
    /// // register own wizard
    /// RegisteredObjects.AddWizard(typeof(MyWizard), myWizBmp, "My Wizard", true);
    /// // register own export filter
    /// RegisteredObjects.AddExport(typeof(MyExport), "My Export");
    /// // register own report object
    /// RegisteredObjects.Add(typeof(MyObject), "ReportPage", myObjBmp, "My Object");
    /// </code>
    /// </example>
    public static partial class RegisteredObjects
    {
        #region Fields
        private static Hashtable FTypes = new Hashtable();
        private static ObjectInfo FObjects = new ObjectInfo();
        private static Dictionary<Type, Dictionary<string, Delegate>> methodsDictionary
            = new Dictionary<Type, Dictionary<string, Delegate>>();
        #endregion

        #region Properties
        /// <summary>
        /// Root object for all registered objects.
        /// </summary>
        public static ObjectInfo Objects
        {
            get { return FObjects; }
        }
        #endregion

        #region Private Methods

        private static void RegisterType(Type type)
        {
            FTypes[type.Name] = type;
        }

        private static void InternalAdd(Object obj, string category, Bitmap image, int imageIndex, string text, int flags,
            bool multiInsert)
        {
            ObjectInfo item = FObjects.FindOrCreate(category);
            item.Update(obj, image, imageIndex, text, flags, multiInsert);
            if (obj is Type)
                RegisterType(obj as Type);
        }

        private static void InternalAdd(Object obj, string category, Bitmap image, int imageIndex, int buttonIndex,
            string text, int flags, bool multiInsert)
        {
            ObjectInfo item = FObjects.FindOrCreate(category);
            item.Update(obj, image, imageIndex, buttonIndex, text, flags, multiInsert);
            if (obj is Type)
                RegisterType(obj as Type);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Checks whether the specified type is registered already.
        /// </summary>
        /// <param name="obj">Type to check.</param>
        /// <returns><b>true</b> if such type is registered.</returns>
        public static bool IsTypeRegistered(Type obj)
        {
            return FTypes.ContainsKey(obj.Name);
        }

        internal static void AddReport(Type obj, int imageIndex)
        {
            InternalAdd(obj, "", null, imageIndex, "", 0, false);
        }

        internal static void AddPage(Type obj, string category, int imageIndex)
        {
            InternalAdd(obj, category, null, imageIndex, "", 0, false);
        }

        internal static void AddCategory(string category, int imageIndex, string text)
        {
            InternalAdd(null, category, null, imageIndex, text, 0, false);
        }

        internal static void AddCategory(string category, int imageIndex, int buttonIndex, string text)
        {
            InternalAdd(null, category, null, imageIndex, buttonIndex, text, 0, false);
        }

        /// <summary>
        /// Registers a category that may contain several report objects.
        /// </summary>
        /// <param name="name">Category name.</param>
        /// <param name="image">Image for category button.</param>
        /// <param name="text">Text for category button.</param>
        /// <remarks>
        /// <para>Category is a button on the "Objects" toolbar that shows context menu with nested items 
        /// when you click it. Consider using categories if you register several report objects. It can 
        /// save space on the "Objects" toolbar. For example, FastReport registers one category called "Shapes"
        /// that contains the <b>LineObject</b> and different types of <b>ShapeObject</b>.</para>
        /// <para>The name of category must starts either with "ReportPage," or "DialogPage," depending on
        /// what kind of controls do you need to regiter in this category: report objects or dialog controls.
        /// After the comma, specify the category name. So the full category name that you need to specify
        /// in the <b>name</b> parameter, must be something like this: "ReportPage,Shapes".
        /// </para>
        /// <para>When register an object inside a category, you must specify the full category name in the
        /// <b>category</b> parameter of the <b>Add</b> method. </para>
        /// </remarks>
        public static void AddCategory(string name, Bitmap image, string text)
        {
            InternalAdd(null, name, image, -1, text, 0, false);
        }

        /// <summary>
        /// Register Export category.
        /// </summary>
        /// <param name="name">Category name.</param>
        /// <param name="text">Category text.</param>
        public static void AddExportCategory(string name, string text)
        {
            InternalAdd(null, "ExportGroups," + name, null, -1, text, 0, false);
        }

        internal static void AddExportCategory(string name, string text, int imageIndex)
        {
            InternalAdd(null, "ExportGroups," + name, null, imageIndex, text, 0, false);
        }

        /// <summary>
        /// Registers a new export filter.
        /// </summary>
        /// <param name="obj">Type of export filter.</param>
        /// <param name="text">Text for export filter's menu item.</param>
        /// <remarks>
        /// The <b>obj</b> must be of <see cref="ExportBase"/> type.
        /// </remarks>
        /// <example>
        /// <code>
        /// // register own export filter
        /// RegisteredObjects.AddExport(typeof(MyExport), "My Export");
        /// </code>
        /// </example>
        public static void AddExport(Type obj, string text)
        {
            if (!obj.IsSubclassOf(typeof(ExportBase)))
                throw new Exception("The 'obj' parameter must be of ExportBase type.");
            InternalAdd(obj, "", null, -1, text, 0, false);
        }

        internal static void AddExport(Type obj, string text, int imageIndex)
        {
            InternalAdd(obj, "", null, imageIndex, text, 0, false);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddExport(Type obj, string category, string text)
        {
            if (!obj.IsSubclassOf(typeof(ExportBase)))
                throw new Exception("The 'obj' parameter must be of ExportBase type.");
            InternalAdd(obj, "ExportGroups," + category + ",", null, -1, text, 0, false);
        }

        internal static void AddExport(Type obj, string category, string text, int imageIndex)
        {
            InternalAdd(obj, "ExportGroups," + category + ",", null, imageIndex, text, 0, false);
        }

        public static void AddExport(Type obj, string category, string text, Bitmap image)
        {
            InternalAdd(obj, "ExportGroups," + category + ",", image, -1, text, 0, false);
        }

        /// <summary>
        /// Registers data connection.
        /// </summary>
        /// <param name="obj">Type of connection.</param>
        /// <remarks>
        /// The <b>obj</b> must be of <see cref="DataConnectionBase"/> type.
        /// </remarks>
        /// <example>
        /// <code>
        /// // register data connection
        /// RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
        /// </code>
        /// </example>
        public static void AddConnection(Type obj)
        {
            AddConnection(obj, "");
        }

        /// <summary>
        /// Registers custom data connection.
        /// </summary>
        /// <param name="obj">Type of connection.</param>
        /// <param name="text">Name of connection.</param>
        /// <remarks>
        /// The <b>obj</b> must be of <see cref="DataConnectionBase"/> type.
        /// </remarks>
        /// <example>
        /// <code>
        /// // register data connection
        /// RegisteredObjects.AddConnection(typeof(MyDataConnection), "My Data Connection");
        /// </code>
        /// </example>
        public static void AddConnection(Type obj, string text)
        {
            if (!obj.IsSubclassOf(typeof(DataConnectionBase)))
                throw new Exception("The 'obj' parameter must be of DataConnectionBase type.");
            if (!IsTypeRegistered(obj))
                Add(obj, "", 0, text);
        }

        /// <summary>
        /// Registers an object in the specified category.
        /// </summary>
        /// <param name="obj">Type of object to register.</param>
        /// <param name="category">Name of category to register in.</param>
        /// <param name="imageIndex">Index of image for object's button.</param>
        public static void Add(Type obj, string category, int imageIndex)
        {
            InternalAdd(obj, category + ",", null, imageIndex, "", 0, false);
        }

        /// <summary>
        /// Registers an object in the specified category.
        /// </summary>
        /// <param name="obj">Type of object to register.</param>
        /// <param name="category">Name of category to register in.</param>
        /// <param name="imageIndex">Index of image for object's button.</param>
        /// <param name="buttonIndex">Index of object's button in toolbar.</param>
        public static void Add(Type obj, string category, int imageIndex, int buttonIndex)
        {
            InternalAdd(obj, category + ",", null, imageIndex, buttonIndex, "", 0, false);
        }

        internal static void Add(Type obj, string category, int imageIndex, string text)
        {
            InternalAdd(obj, category + ",", null, imageIndex, text, 0, false);
        }

        internal static void Add(Type obj, string category, int imageIndex, string text, int flags)
        {
            InternalAdd(obj, category + ",", null, imageIndex, text, flags, false);
        }

        internal static void Add(Type obj, string category, int imageIndex, string text, int flags, bool multiInsert)
        {
            InternalAdd(obj, category + ",", null, imageIndex, text, flags, multiInsert);
        }

        /// <summary>
        /// Registers an object in the specified category with button's image and text.
        /// </summary>
        /// <param name="obj">Type of object to register.</param>
        /// <param name="category">Name of category to register in.</param>
        /// <param name="image">Image for object's button.</param>
        /// <param name="text">Text for object's button.</param>
        /// <remarks>
        /// <para>You must specify either the page type name or existing category name in the <b>category</b> parameter.
        /// The report objects must be registered in the "ReportPage" category or custom category that is
        /// registered in the "ReportPage" as well. The dialog controls must be registered in the "DialogPage"
        /// category or custom category that is registered in the "DialogPage" as well.</para>
        /// <para>If you want to register an object that needs to be serialized, but you don't want
        /// to show it on the toolbar, pass empty string in the <b>category</b> parameter.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // register the report object
        /// RegisteredObjects.Add(typeof(MyReportObject), "ReportPage", myReportObjectBmp, "My Report Object");
        /// // register the dialog control
        /// RegisteredObjects.Add(typeof(MyDialogControl), "DialogPage", myDialogControlBmp, "My Dialog Control");
        /// // add a category and register an object inside it
        /// RegisteredObjects.AddCategory("ReportPage,MyCategory", myCategoryBmp, "My Category");
        /// // register another report object in MyCategory
        /// RegisteredObjects.Add(typeof(MyReportObject), "ReportPage,MyCategory", 
        ///   anotherReportObjectBmp, "Another Report Object");
        /// </code>
        /// </example>
        public static void Add(Type obj, string category, Bitmap image, string text)
        {
            InternalAdd(obj, category + ",", image, -1, text, 0, false);
        }

        /// <summary>
        /// Registers an object in the specified category with button's image, text and object's flags.
        /// </summary>
        /// <param name="obj">Type of object to register.</param>
        /// <param name="category">Name of category to register in.</param>
        /// <param name="image">Image for object's button.</param>
        /// <param name="text">Text for object's button.</param>
        /// <param name="flags">Integer value that will be passed to object's <b>OnBeforeInsert</b> method.</param>
        /// <remarks>
        /// <para>See the <see cref="Add(Type,string,Bitmap,string)"/> method for more details.</para>
        /// <para>To learn about flags, see the <see cref="Base.OnBeforeInsert"/> method.</para>
        /// </remarks>
        public static void Add(Type obj, string category, Bitmap image, string text, int flags)
        {
            InternalAdd(obj, category + ",", image, -1, text, flags, false);
        }

        /// <summary>
        /// Registers an object in the specified category with button's image, text, object's flags and multi-insert flag.
        /// </summary>
        /// <param name="obj">Type of object to register.</param>
        /// <param name="category">Name of category to register in.</param>
        /// <param name="image">Image for object's button.</param>
        /// <param name="text">Text for object's button.</param>
        /// <param name="flags">Integer value that will be passed to object's <b>OnBeforeInsert</b> method.</param>
        /// <param name="multiInsert">Specifies whether the object may be inserted several times until you
        /// select the "arrow" button or insert another object.</param>
        /// <remarks>
        /// <para>See the <see cref="Add(Type,string,Bitmap,string)"/> method for more details.</para>
        /// <para>To learn about flags, see the <see cref="Base.OnBeforeInsert"/> method.</para>
        /// </remarks>
        public static void Add(Type obj, string category, Bitmap image, string text, int flags, bool multiInsert)
        {
            InternalAdd(obj, category + ",", image, -1, text, flags, multiInsert);
        }

        /// <summary>
        /// Adds a new function category.
        /// </summary>
        /// <param name="category">Short name of category.</param>
        /// <param name="text">Display name of category.</param>
        /// <remarks>
        /// Short name is used to reference the category in the subsequent <see cref="AddFunction"/> 
        /// method call. It may be any value, for example, "MyFuncs". Display name of category is displayed 
        /// in the "Data" window. In may be, for example, "My Functions".
        /// <para/>The following standard categories are registered by default:
        /// <list type="bullet">
        ///   <item>
        ///     <description>"Math"</description>
        ///   </item>
        ///   <item>
        ///     <description>"Text"</description>
        ///   </item>
        ///   <item>
        ///     <description>"DateTime"</description>
        ///   </item>
        ///   <item>
        ///     <description>"Formatting"</description>
        ///   </item>
        ///   <item>
        ///     <description>"Conversion"</description>
        ///   </item>
        ///   <item>
        ///     <description>"ProgramFlow"</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// This example shows how to register a new category:
        /// <code>
        /// RegisteredObjects.AddFunctionCategory("MyFuncs", "My Functions");
        /// </code>
        /// </example>
        public static void AddFunctionCategory(string category, string text)
        {
            InternalAdd(null, "Functions," + category, null, 66, text, 0, false);
        }

        /// <summary>
        /// Adds a new function into the specified category.
        /// </summary>
        /// <param name="function"><b>MethodInfo</b> containing all necessary information about the function.</param>
        /// <param name="category">The name of category to register the function in.</param>
        /// <remarks>
        /// Your function must be a static, public method of a public class.
        /// <para/>The following standard categories are registered by default:
        /// <list type="bullet">
        ///   <item>
        ///     <description>"Math"</description>
        ///   </item>
        ///   <item>
        ///     <description>"Text"</description>
        ///   </item>
        ///   <item>
        ///     <description>"DateTime"</description>
        ///   </item>
        ///   <item>
        ///     <description>"Formatting"</description>
        ///   </item>
        ///   <item>
        ///     <description>"Conversion"</description>
        ///   </item>
        ///   <item>
        ///     <description>"ProgramFlow"</description>
        ///   </item>
        /// </list>
        /// You may use one of the standard categories, or create a new category by the
        /// <see cref="AddFunctionCategory"/> method call.
        /// <para/>FastReport uses XML comments to display your function's description. 
        /// To generate XML comments, enable it in your project's properties 
        /// ("Project|Properties..." menu, "Build" tab, enable the "XML documentation file" checkbox).
        /// </remarks>
        /// <example>
        /// The following example shows how to register own functions:
        /// <code>
        /// public static class MyFunctions
        /// {
        ///   /// &lt;summary&gt;
        ///   /// Converts a specified string to uppercase.
        ///   /// &lt;/summary&gt;
        ///   /// &lt;param name="s"&gt;The string to convert.&lt;/param&gt;
        ///   /// &lt;returns&gt;A string in uppercase.&lt;/returns&gt;
        ///   public static string MyUpperCase(string s)
        ///   {
        ///     return s == null ? "" : s.ToUpper();
        ///   }
        ///
        ///   /// &lt;summary&gt;
        ///   /// Returns the larger of two 32-bit signed integers. 
        ///   /// &lt;/summary&gt;
        ///   /// &lt;param name="val1"&gt;The first of two values to compare.&lt;/param&gt;
        ///   /// &lt;param name="val2"&gt;The second of two values to compare.&lt;/param&gt;
        ///   /// &lt;returns&gt;Parameter val1 or val2, whichever is larger.&lt;/returns&gt;
        ///   public static int MyMaximum(int val1, int val2)
        ///   {
        ///     return Math.Max(val1, val2);
        ///   }
        ///
        ///   /// &lt;summary&gt;
        ///   /// Returns the larger of two 64-bit signed integers. 
        ///   /// &lt;/summary&gt;
        ///   /// &lt;param name="val1"&gt;The first of two values to compare.&lt;/param&gt;
        ///   /// &lt;param name="val2"&gt;The second of two values to compare.&lt;/param&gt;
        ///   /// &lt;returns&gt;Parameter val1 or val2, whichever is larger.&lt;/returns&gt;
        ///   public static long MyMaximum(long val1, long val2)
        ///   {
        ///     return Math.Max(val1, val2);
        ///   }
        /// }
        /// 
        /// // register a category
        /// RegisteredObjects.AddFunctionCategory("MyFuncs", "My Functions");
        ///  
        /// // obtain MethodInfo for our functions
        /// Type myType = typeof(MyFunctions);
        /// MethodInfo myUpperCaseFunc = myType.GetMethod("MyUpperCase");
        /// MethodInfo myMaximumIntFunc = myType.GetMethod("MyMaximum", new Type[] { typeof(int), typeof(int) });
        /// MethodInfo myMaximumLongFunc = myType.GetMethod("MyMaximum", new Type[] { typeof(long), typeof(long) });
        ///      
        /// // register simple function
        /// RegisteredObjects.AddFunction(myUpperCaseFunc, "MyFuncs");
        ///      
        /// // register overridden functions
        /// RegisteredObjects.AddFunction(myMaximumIntFunc, "MyFuncs,MyMaximum");
        /// RegisteredObjects.AddFunction(myMaximumLongFunc, "MyFuncs,MyMaximum");
        /// </code>
        /// </example>
        public static void AddFunction(MethodInfo function, string category)
        {
            if (function == null)
                throw new ArgumentNullException("function");
            InternalAdd(function, "Functions," + category + ",", null, 52, "", 0, false);
        }

        internal static Type FindType(string typeName)
        {
            if (typeName != null && typeName != "")
            {
                return FTypes[typeName] as Type;
            }
            return null;
        }

        /// <summary>
        /// Finds the registered object's info.
        /// </summary>
        /// <param name="type">The type of object to find.</param>
        /// <returns>The object's info.</returns>
        /// <remarks>This method can be used to disable some objects, for example:
        /// <para/>RegisteredObjects.FindObject(typeof(PDFExport)).Enabled = false;
        /// </remarks>
        public static ObjectInfo FindObject(Type type)
        {
            if (type == null)
                return null;

            List<ObjectInfo> list = new List<ObjectInfo>();
            FObjects.EnumItems(list);

            foreach (ObjectInfo item in list)
            {
                if (item.Object == type)
                    return item;
            }
            return null;
        }

        internal static ObjectInfo FindObject(object obj)
        {
            if (obj == null)
                return null;
            return FindObject(obj.GetType());
        }


        /// <summary>
        /// Register and override the method with method name in the type.
        /// For property use the property name and _Get or _Set suffix.
        /// </summary>
        /// <param name="type">Type for registering method</param>
        /// <param name="methodName">Name of method fir registering</param>
        /// <param name="method">Method for registering</param>
        public static void RegisterMethod(Type type, string methodName, Delegate method)
        {
            Dictionary<string, Delegate> methods;
            if (!methodsDictionary.TryGetValue(type, out methods))
            {
                methods = new Dictionary<string, Delegate>();
                methodsDictionary[type] = methods;
            }

            methods[methodName] = method;
        }

        /// <summary>
        /// Gets the method or null if method is not found
        /// </summary>
        /// <param name="type">Type for method finding</param>
        /// <param name="methodName">Name for method finfing</param>
        /// <param name="inheritance">Use True value for inheritance the method from base type, use false for get the method only from the this type</param>
        /// <returns></returns>
        public static Delegate GetMethod(Type type, string methodName, bool inheritance)
        {
            if (type == typeof(object))
                return null;
            Dictionary<string, Delegate> methods;
            if (methodsDictionary.TryGetValue(type, out methods))
            {
                Delegate result;
                if (methods.TryGetValue(methodName, out result))
                    return result;
            }
            if (inheritance)
                return RegisteredObjects.GetMethod(type.BaseType, methodName, inheritance);
            return null;
        }

        #endregion
    }

}