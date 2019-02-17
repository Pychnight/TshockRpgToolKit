using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.PluginSupport
{
	/// <summary>
	/// Collection of Error and Warning information for types that undergo validation procedures.
	/// </summary>
	public class ValidationResult
	{
		/// <summary>
		/// Gets a List of ValidationErrors.
		/// </summary>
		public IList<ValidationError> Errors { get; private set; }
		/// <summary>
		/// Gets a List of ValidationWarnings.
		/// </summary>
		public IList<ValidationWarning> Warnings { get; private set; }
		/// <summary>
		/// Gets a List of ValidationResults, from any sub objects.  
		/// </summary>
		public IList<ValidationResult> Children { get; private set; }
		/// <summary>
		/// Gets a value determining if <see cref="Errors"/> contains any <see cref="ValidationError"/>s.
		/// </summary>
		public bool HasErrors => Errors.Count > 0;
		/// <summary>
		/// Gets a value determining if <see cref="Warnings"/> contains any <see cref="ValidationWarning"/>s.
		/// </summary>
		public bool HasWarnings => Warnings.Count > 0;
		/// <summary>
		/// Gets a value determining if this ValidationResult has any child results.
		/// </summary>
		public bool HasChildResults => Children.Count > 0;
		/// <summary>
		/// Gets or sets an optional object that refers to the origin of this <see cref="ValidationResult" />. 
		/// </summary>
		/// <remarks>Source may refer to a file name, or an object, or some other contextual hint. For outputting purposes, Source.ToString()
		/// would typically used.</remarks>
		public object Source { get; set; }

		public ValidationResult() : this(null) { }

		public ValidationResult(object source)
		{
			Errors = new ValidationResultItemCollection<ValidationError>(this);
			Warnings = new ValidationResultItemCollection<ValidationWarning>(this);
			Children = new List<ValidationResult>();
			Source = source;
		}
		
		/// <summary>
		/// Set all warnings and errors to a new source.
		/// </summary>
		/// <param name="source">New source.</param>
		/// <param name="setErrors">True to set error sources.</param>
		/// <param name="setWarnings">True to set warning sources.</param>
		public void SetSources(string source, bool setErrors = true, bool setWarnings = true, bool setChildren = false )
		{
			if(setErrors)
			{
				foreach(var i in Errors)
					i.Source = source;
			}

			if(setWarnings)
			{
				foreach(var i in Warnings)
					i.Source = source;
			}

			if(setChildren)
			{
				foreach (var child in Children)
					child.SetSources(source, setErrors, setWarnings, setChildren);
			}
		}

		/// <summary>
		/// Recursively sums all errors and warnings for this <see cref="ValidationResult"/>, and all <see cref="Children"/>.
		/// </summary>
		/// <param name="totalErrors"></param>
		/// <param name="totalWarnings"></param>
		public void GetTotals(ref int totalErrors, ref int totalWarnings)
		{
			totalErrors += Errors.Count;
			totalWarnings += Warnings.Count;

			foreach(var child in Children)
				child.GetTotals(ref totalErrors, ref totalWarnings);
		}

		/// <summary>
		/// Custom collection that sets/unsets the Parent <see cref="ValidationResult" /> for items.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class ValidationResultItemCollection<T> : Collection<T> where T : ValidationResultItem
		{
			ValidationResult parent;

			internal ValidationResultItemCollection(ValidationResult parent)
			{
				this.parent = parent;
			}

			protected override void InsertItem(int index, T item)
			{
				base.InsertItem(index, item);
				item.Parent = parent;
			}

			protected override void RemoveItem(int index)
			{
				var item = this[index];
				item.Parent = null;
				base.RemoveItem(index);
			}
		}
	}
}
