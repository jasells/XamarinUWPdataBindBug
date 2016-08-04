using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace XamarinUWPdataBindBug
{
    public class BindablePicker : Picker
    {
        #region Constructor

        public BindablePicker()
        {
            SelectedIndexChanged += OnSelectionChanged;
        }

        #endregion Constructor

        #region Fields

        //Bindable property for the items source
        public static readonly BindableProperty DisplayMemberBindingProperty =
            BindableProperty.Create<BindablePicker, string>(p => p.DisplayMemberBinding, string.Empty, BindingMode.TwoWay);

        //Bindable property for the selected item
        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create<BindablePicker, object>(p => p.SelectedItem, null, BindingMode.TwoWay, propertyChanged: OnSelectedItemPropertyChanged);

        //Bindable property for the items source
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create<BindablePicker, IList>(
                                        p => p.ItemsSource, null,
                                        BindingMode.OneWay, 
                                        propertyChanged: OnItemsSourcePropertyChanged);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>
        /// The items source.
        /// </value>
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string DisplayMemberBinding
        {
            get { return GetValue(DisplayMemberBindingProperty).ToString(); }
            set { SetValue(DisplayMemberBindingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Called when [items source property changed].
        /// </summary>
        /// <param name="bindable">The bindable.</param>
        /// <param name="value">The value.</param>
        /// <param name="newValue">The new value.</param>
        private static void OnItemsSourcePropertyChanged(BindableObject bindable, IEnumerable value, IEnumerable newValue)
        {
            var picker = (BindablePicker)bindable;
            var eventList = picker.GetType().GetRuntimeEvents();
            var notifyCollection = newValue as INotifyCollectionChanged;
            //var page = picker.Parent as ContentPage;
            //page.Appearing += OnPageAppearing;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged += (sender, args) =>
                {
                    if (args.NewItems != null)
                    {
                        //invoke on main thread here?
                        foreach (var newItem in args.NewItems)
                        {
                            picker.Items.Add((newItem ?? "").ToString());
                        }
                    }
                    if (args.OldItems != null)
                    {
                        //invoke on main thread here?
                        foreach (var oldItem in args.OldItems)
                        {
                            picker.Items.Remove((oldItem ?? "").ToString());
                        }
                    }
                };
            }

            if (newValue == null)
                return;

            //invoke here?
            picker.Items.Clear();

            if (string.IsNullOrWhiteSpace(picker.DisplayMemberBinding))
                foreach (var item in newValue)
                    picker.Items.Add(item?.ToString());
            else
                foreach (var item in newValue)
                {
                    try
                    {
                        List<FieldInfo> fields = item.GetType().GetRuntimeFields().ToList();
                        var actualValue = string.Empty;
                        foreach (FieldInfo field in fields.ToList())
                        {
                            if (field.Name.Contains(picker.DisplayMemberBinding))
                            {
                                actualValue = field.GetValue(item).ToString();
                            }
                        }
                        //invoke here?
                        picker.Items.Add((actualValue ?? "").ToString());
                    }
                    catch (System.Exception)
                    {
                        throw;
                    }
                }
        }

        /// <summary>
        /// Called when [selected item property changed].
        /// </summary>
        /// <param name="bindable">The bindable.</param>
        /// <param name="value">The value.</param>
        /// <param name="newValue">The new value.</param>
        private static void OnSelectedItemPropertyChanged(BindableObject bindable, object value, object newValue)
        {
            var picker = (BindablePicker)bindable;
            if (picker.ItemsSource == null) return;
            var index = picker.ItemsSource.IndexOf(picker.SelectedItem);
            if (picker.SelectedIndex == index)
                return;
            picker.SelectedIndex = index;
        }

        private void OnSelectionChanged(object sender, System.EventArgs e)
        {
            if (SelectedIndex < 0 || SelectedIndex > Items.Count - 1)
            {
                SelectedItem = null;
            }
            else
            {
                int count = 0;
                foreach (var item in ItemsSource)
                {
                    if (count == SelectedIndex)
                    {
                        SelectedItem = item;
                        break;
                    }
                    ++count;
                }
            }
        }

        #endregion Methods
    }
}