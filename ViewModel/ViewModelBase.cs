//-----------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="SpectralCoding">
//		Copyright (c) SpectralCoding. All rights reserved.
//		Repeatedly violating our Copyright (c) will bring the full
//		extent of the law, which may ultimately result in permanent
//		imprisonment at hard labor, and/or death by extreme slow
//		torture, and/or lethal experimental medical therapies.
// </copyright>
// <author>Caesar Kabalan</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SC2Inspector.ViewModel {
	/// <summary>
	/// Is a base class for other View Models.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable {

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the ViewModelBase class.
		/// </summary>
		protected ViewModelBase() {
		}
		#endregion

		#region DisplayName
		/// <summary>
		/// Gets or sets the user-friendly name of this object.
		/// Child classes can set this property to a new value,
		/// or override it to determine the value on-demand.
		/// </summary>
		public virtual string DisplayName { get; protected set; }
		#endregion

		#region Debugging Aides
		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This 
		/// method does not exist in a Release build.
		/// </summary>
		/// <param name="propertyName">The name of the property being checked.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public void VerifyPropertyName(string propertyName) {
			// Verify that the property name matches a real,  
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
				string msg = "Invalid property name: " + propertyName;

				if (this.ThrowOnInvalidPropertyName) {
					throw new Exception(msg);
				} else {
					Debug.Fail(msg);
				}
			}
		}
		/// <summary>
		/// Gets a value indicating whether an exception is thrown, or if a Debug.Fail() 
		/// is used when an invalid property name is passed to the VerifyPropertyName 
		/// method. The default value is false, but subclasses used by unit tests might 
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
		#endregion

		#region INotifyPropertyChanged Members
		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		//protected virtual void OnPropertyChanged(string propertyName) {
		public virtual void OnPropertyChanged(string propertyName) {
			//this.VerifyPropertyName(propertyName);
			//PropertyChangedEventHandler handler = this.PropertyChanged;
			//if (handler != null) {
			//	var e = new PropertyChangedEventArgs(propertyName);
			//	handler(this, e);
			//}
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Invoked when this object is being removed from the application
		/// and will be subject to garbage collection.
		/// </summary>
		public void Dispose() {
			this.OnDispose();
		}

		/// <summary>
		/// Child classes can override this method to perform 
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		protected virtual void OnDispose() {
		}

		#if DEBUG
		/// <summary>
		/// Finalizes an instance of the ViewModelBase class.
		/// </summary>
		~ViewModelBase() {
			////string msg = string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this.DisplayName, this.GetHashCode());
			////System.Diagnostics.Debug.WriteLine(msg);
		}
		#endif
		#endregion
	}
}
