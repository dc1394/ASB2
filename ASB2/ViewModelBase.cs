//-----------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="dc1394's software">
//     but this is originally adapted by id:minami_SC
//     cf. http://sourcechord.hatenablog.com/entry/2014/06/08/123738
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    #region ==== Abstract Class : ViewModelBase

    /// <summary>
    /// ViewModelの抽象基底クラス
    ///    INotifyPropertyChanged, IDataErrorInfo を実装、
    ///    その他 ViewModel用のヘルパークラス等を提供する
    /// </summary>
    internal abstract class ViewModelBase : MyViewModelBase.BindableBase, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region 発生中のエラーを保持する処理を実装

        private readonly Dictionary<string, List<string>> _currentErrors = new Dictionary<string, List<string>>();

        protected void AddError(string propertyName, string error)
        {
            if (!this._currentErrors.ContainsKey(propertyName))
            {
                this._currentErrors[propertyName] = new List<string>();
            }

            if (!this._currentErrors[propertyName].Contains(error))
            {
                this._currentErrors[propertyName].Add(error);
                this.OnErrorsChanged(propertyName);
            }
        }

        protected void RemoveError(string propertyName)
        {
            if (this._currentErrors.ContainsKey(propertyName))
            {
                this._currentErrors.Remove(propertyName);
            }

            this.OnErrorsChanged(propertyName);
        }
        #endregion

        private void OnErrorsChanged(string propertyName)
        {
            var h = this.ErrorsChanged;
            if (h != null)
            {
                h(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #region INotifyDataErrorInfoの実装
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !this._currentErrors.ContainsKey(propertyName))
            {
                return null;
            }

            return this._currentErrors[propertyName];
        }

        public bool HasErrors
        {
            get { return this._currentErrors.Count > 0; }
        }

        #endregion

        #region **** Inner Class : DataBindItemBase
        /// <summary>
        /// データバインド項目抽象基底クラス
        /// </summary>
        protected abstract class DataBindItemBase
        {
            #region **** Members

            /// <summary>
            /// 値が変更されたかどうかを保持する内部変数
            /// </summary>
            private bool _changed;

            #endregion

            #region **** Properties

            /// <summary>
            /// 親となるViewModel基底クラスへの参照 : ViewModelBase
            /// </summary>
            protected ViewModelBase _VM { get; set; }

            /// <summary>
            /// 項目名 : string
            /// </summary>
            public string Name { get; protected set; }

            /// <summary>
            /// 値の型(抽象) : Type
            /// </summary>
            public abstract Type ValueType { get; }

            /// <summary>
            /// 値が変更された？ : bool
            /// </summary>
            public bool Changed
            {
                get
                {
                    return this._changed;
                }

                protected set
                {
                    if (this._changed != value)
                    {
                        this._VM.SetProperty(ref this._changed, value);
                    }
                }
            }

            /// <summary>
            /// 不正な値を受け付ける？
            ///    Trueの場合、ValidationCheckでエラーになった値もValueへの代入を認める
            /// </summary>
            public bool AcceptInvalidData { get; set; }

            #endregion

            #region **** Abstract Method : Initialize
            /// <summary>
            /// 初期処理（抽象）
            /// </summary>
            public abstract void Initialize();
            #endregion

            #region **** ClearChanged
            /// <summary>
            /// 項目を未変更状態にする
            /// </summary>
            public void ClearChanged()
            {
                this.Changed = false;
            }
            #endregion
        }
        #endregion

        #region **** Inner Class : DataBindItem<T>
        /// <summary>
        /// データバインド項目クラス（ジェネリック）
        /// </summary>
        /// <typeparam name="T">項目の型</typeparam>
        protected class DataBindItem<T> : DataBindItemBase
        {
            #region **** Members

            /// <summary>
            /// 項目の値を保持する内部変数
            /// </summary>
            private T _value;

            /// <summary>
            /// 項目の初期値を保持する内部変数
            /// </summary>
            private T _defaultValue;

            #endregion

            #region **** Properties

            /// <summary>
            /// 項目値 : T
            /// </summary>
            public T Value
            {
                get
                {
                    return this._value;
                }

                set
                {
                    // 値が変更されたときのみ処理する
                    if (!this.Eq(this._value, value))
                    {
                        // 値を代入する
                        this._value = value;
                        this._VM.RaisePropertyChanged(this.Name);
                        this.Changed = true;
                    }
                }
            }

            /// <summary>
            /// 項目の型 : Type
            /// </summary>
            public override Type ValueType
            {
                get { return this._value.GetType(); }
            }

            #endregion

            #region **** Constructor
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="vm">ViewModelへの参照 : ViewModelBase</param>
            /// <param name="name">項目名 : string</param>
            /// <param name="defaultValue">初期値 : T</param>
            public DataBindItem(ViewModelBase vm, string name, T defaultValue)
            {
                if (vm == null || string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException();
                }

                this._VM = vm;
                this.Name = name;
                this._value = this._defaultValue = defaultValue;
                this.Changed = false;
                this.AcceptInvalidData = false;
            }
            #endregion

            #region **** Private Method : Eq
            /// <summary>
            /// 値が一致するか？
            ///     Tに対して直接比較演算子を使用できないためIComparableにキャストして比較する
            ///     IComparableでない場合はつねにFalseを返す。
            /// </summary>
            /// <param name="v1">値1 : T</param>
            /// <param name="v2">値2 : T</param>
            /// <returns>結果値 : bool</returns>
            private bool Eq(T v1, T v2)
            {
                if (v1 is IComparable)
                {
                    return (v1 as IComparable).CompareTo(v2) == 0;
                }
                else
                {
                    return false;
                }
            }
            #endregion

            #region **** Method (Override) : Initialize
            /// <summary>
            /// 項目を初期化する
            ///     コンストラクタで指定した初期値がセットされる。
            ///     エラー、変更状態もクリアされる。
            /// </summary>
            public override void Initialize()
            {
                this._value = this._defaultValue;
                this.Changed = false;
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// DataBindItemのコレクション 
        /// </summary>
        protected Dictionary<string, DataBindItemBase> DataBindItems { get; private set; }

        #region **** Constructor
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ViewModelBase()
        {
            this.DataBindItems = new Dictionary<string, DataBindItemBase>();
        }
        #endregion

        #region **** Method : CreateDataBindItem(1)
        /// <summary>
        /// データバインド項目の生成
        /// </summary>
        /// <typeparam name="T">項目の型</typeparam>
        /// <param name="name">項目名 : string</param>
        /// <param name="defaultValue">初期値 : T</param>
        /// <returns>項目インスタンス : DataBindItem</returns>
        protected DataBindItem<T> CreateDataBindItem<T>(string name, T defaultValue)
        {
            DataBindItem<T> item = new DataBindItem<T>(this, name, defaultValue);
            this.DataBindItems[name] = item;
            return item;
        }
        #endregion

        #region **** Method : CreateDataBindItem(2)
        /// <summary>
        /// データバインド項目の生成
        /// </summary>
        /// <typeparam name="T">項目の型</typeparam>
        /// <param name="name">項目名 : string</param>
        /// <param name="defaultValue">初期値 : T</param>
        /// <param name="acceptInvalidData">不正な値を受け入れるか？ : bool</param>
        /// <returns>項目インスタンス : DataBindItem</returns>
        protected DataBindItem<T> CreateDataBindItem<T>(string name, T defaultValue, bool acceptInvalidData)
        {
            DataBindItem<T> item = new DataBindItem<T>(this, name, defaultValue);
            item.AcceptInvalidData = acceptInvalidData;
            this.DataBindItems[name] = item;
            return item;
        }
        #endregion

        #region **** Method : GetDataBindItem
        /// <summary>
        /// データバインド項目の取得
        /// </summary>
        /// <typeparam name="T">項目の型</typeparam>
        /// <param name="name">項目名 : string</param>
        /// <returns>項目インスタンス : DataBindItem</returns>
        protected DataBindItem<T> GetDataBindItem<T>(string name)
        {
            return this.DataBindItems[name] as DataBindItem<T>;
        }
        #endregion

        protected void ValidateProperty(string propertyName, object value)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var validationErrors = new List<ValidationResult>();
            if (!Validator.TryValidateProperty(value, context, validationErrors))
            {
                var errors = validationErrors.Select(error => error.ErrorMessage);
                foreach (var error in errors)
                {
                    this.AddError(propertyName, error);
                }
            }
            else
            {
                this.RemoveError(propertyName);
            }
        }
    }

    #endregion
}
