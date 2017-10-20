using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using System.Linq;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using SPromoterMobile;

namespace spromotermobile.droid
{
    public class UIFormDinamico
    {
        readonly FormDinamico context;
        readonly LinearLayout layout;
        readonly FormDinamicoModel model;
        FormDinamicoCon Controller { get; }

        public UIFormDinamico(FormDinamico context, LinearLayout layout,
                              FormDinamicoCon controller)
        {
            try
            {
                this.context = context;
                this.layout = layout;
                this.Controller = controller;
                model = controller.Model;
                IniForms();
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Referencias do UIFormDinamico null");
            }
        }

        void IniForms()
        {
            var layoutParams = (ViewGroup.MarginLayoutParams)layout.LayoutParameters;
            layoutParams.RightMargin = (int)context.Resources.GetDimension(Resource.Dimension.activity_vertical_margin);
            layoutParams.LeftMargin = (int)context.Resources.GetDimension(Resource.Dimension.activity_vertical_margin);
            layoutParams.TopMargin = (int)context.Resources.GetDimension(Resource.Dimension.activity_vertical_margin);
            layoutParams.BottomMargin = (int)context.Resources.GetDimension(Resource.Dimension.activity_vertical_margin);
            layout.LayoutParameters = layoutParams;
            TextView txtDescription;
            EditText ediText;
            for (int item = 0; item <= model.CamposForm.campos.Count - 1; item++)
            {
                var forms = model.CamposForm.campos[item].tipo;
                var minCaract = model.CamposForm.campos[item].minCaracteres;
                var maxCaract = model.CamposForm.campos[item].maxCaracteres;
                var conteudoString = model.CamposForm.campos[item].conteudo;
                switch (forms)
                {
                    case (0):
                        txtDescription = new TextView(context);
                        IniLabel(txtDescription);
                        ediText = new EditText(context);
                        ediText.SetMaxLines(1);
                        ediText.InputType = InputTypes.TextFlagMultiLine;
                        IniEditText(ediText, txtDescription, model.CamposForm.campos[item].descricao, minCaract, maxCaract);
                        if (!string.IsNullOrEmpty(conteudoString))
                        {
                            ediText.Text = conteudoString;
                        }
                        layout.AddView(ediText);
                        ediText.Tag = 0;
                        break;
                    case (1):
                        txtDescription = new TextView(context);
                        IniLabel(txtDescription);
                        ediText = new EditText(context);
                        ediText.SetMaxLines(1);
                        ediText.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
                        IniEditText(ediText, txtDescription, model.CamposForm.campos[item].descricao, minCaract, maxCaract);
                        if (!string.IsNullOrEmpty(conteudoString))
                        {
                            ediText.Text = conteudoString;
                        }
                        layout.AddView(ediText);
                        ediText.Tag = 1;
                        break;
                    case (2):
                        if (!model.CamposForm.campos[item].descricao.ToUpper().Equals("RUPTURA"))
                        {
                            var chck = new CheckBox(context)
                            {
                                Text = model.CamposForm.campos[item].descricao
                            };
                            chck.SetTextSize(ComplexUnitType.Sp, 20);
                            chck.SetTextColor(ContextCompat.GetColorStateList(context, Resource.Color.verdeazulescuro));
                            var id = Resources.System.GetIdentifier("btn_check_holo_light", "drawable", "android");
                            chck.SetButtonDrawable(id);
                            layout.AddView(chck);
                            chck.Tag = 2;
                            var checkString = model.CamposForm.campos[item].conteudo;
                            if (checkString != null)
                            {
                                chck.Checked = Convert.ToBoolean(checkString);
                            }
                            chck.Post(delegate
                            {
                                var layoutCheck = (ViewGroup.MarginLayoutParams)chck.LayoutParameters;
                                layoutCheck.TopMargin = (int)context.Resources.GetDimension(Resource.Dimension.padding_default);
                                layoutCheck.BottomMargin = (int)context.Resources.GetDimension(Resource.Dimension.padding_default);
                                chck.LayoutParameters = layoutCheck;
                            });
                        }
                        break;
                    case (3):
                        txtDescription = new TextView(context);
                        IniLabel(txtDescription);
                        var datePickerText = new EditText(context);
                        datePickerText.SetMaxLines(1);
                        IniEditText(datePickerText, txtDescription, model.CamposForm.campos[item].descricao, minCaract, maxCaract);
                        datePickerText.Focusable = false;
                        if (!datePickerText.HasOnClickListeners)
                        {
                            datePickerText.Click += delegate
                            {
                                var dateListener = new OnDateSetListener(datePickerText);
                                new DatePickerDialog(context, Resource.Style.DialogTheme, dateListener,
                                                        DateTime.Now.Year,
                                                        DateTime.Now.Month,
                                                        DateTime.Now.Day).Show();
                            };
                        }
                        if (!string.IsNullOrEmpty(conteudoString))
                        {
                            datePickerText.Text = conteudoString;
                        }
                        layout.AddView(datePickerText);
                        datePickerText.Tag = 3;
                        break;
                    case (4):
                        txtDescription = new TextView(context);
                        IniLabel(txtDescription);
                        ediText = new EditText(context);
                        ediText.SetMaxLines(1);
                        IniEditText(ediText, txtDescription, model.CamposForm.campos[item].descricao, minCaract, 3);
                        if (!string.IsNullOrEmpty(conteudoString))
                        {
                            ediText.Text = conteudoString;
                        }
                        ediText.Focusable = false;
                        layout.AddView(ediText);
                        if (!ediText.HasOnClickListeners)
                        {
                            var index = layout.ChildCount - 1;
                            ediText.Click += (sender, e) => { ShowPopUpSeekBar(index); };
                        }
                        ediText.Tag = 4;
                        break;
                    case (5):
                        var txt = new TextView(context);
                        IniLabel(txt);
                        var spinnerArray = new List<string>
                        {
                            "Selecione..."
                        };
                        foreach (var spinnerItem in model.CamposForm.campos[item].opcoes)
                        {
                            try
                            {
                                if (spinnerItem != null)
                                {
                                    var spinnerItemString = Convert.ToString(spinnerItem);
                                    spinnerArray.Add(spinnerItemString);
                                }
                            }
                            catch (InvalidCastException) { /*Deu ruim na hora de converter tiu*/ }

                        }
                        var spinner = new Spinner(context);
                        var spinnerArrayAdapter = new ArrayAdapter(context,
                            Resource.Layout.support_simple_spinner_dropdown_item, spinnerArray);
                        spinner.SetBackgroundResource(Resource.Drawable.spinner_background);
                        if (Build.VERSION.SdkInt > BuildVersionCodes.IceCreamSandwich)
                        {
#pragma warning disable XA0001 // Find issues with Android API usage
                            spinner.SetPopupBackgroundDrawable(ContextCompat.GetDrawable(context, Resource.Drawable.spinner_background));
#pragma warning restore XA0001 // Find issues with Android API usage
                        }
                        spinner.Adapter = spinnerArrayAdapter;
                        if (!string.IsNullOrEmpty(conteudoString))
                        {
                            var indexSelected = spinnerArray.IndexOf(conteudoString);
                            if (indexSelected > 0)
                            {
                                spinner.SetSelection(indexSelected);
                            }
                        }
                        layout.AddView(spinner);
                        if (minCaract > 0)
                        {
                            txt.Text = model.CamposForm.campos[item].descricao + " *";

                        }
                        else
                        {
                            txt.Text = model.CamposForm.campos[item].descricao;
                        }
                        spinner.Tag = 5;
                        layout.Post(delegate
                        {
                            var layoutCheck = (ViewGroup.MarginLayoutParams)spinner.LayoutParameters;
                            layoutCheck.TopMargin = (int)context.Resources.GetDimension(Resource.Dimension.padding_default);
                            layoutCheck.BottomMargin = (int)context.Resources.GetDimension(Resource.Dimension.padding_default);
                            spinner.LayoutParameters = layoutCheck;
                        });
                        break;
                    default:
                        throw new InvalidCastException("Tipo nao reconhecido no formulário");
                }
            }

        }

        internal void UpdateValues()
        {
            EditText edittext;
            TextView txtLabel;
            for (var i = 0; i <= layout.ChildCount; i++)
            {
                var v = layout.GetChildAt(i);
                int forms;
                try
                {
                    forms = (int)v.Tag;
                }
                catch (NullReferenceException) { forms = 99; }
                try
                {
                    //Spinner
                    if (forms == 5)
                    {
                        txtLabel = (TextView)layout.GetChildAt(i - 1);
                        var row = Controller.Model.CamposForm.campos.FirstOrDefault(
                            (SPromoterMobile.Models.RESTful.FormSchemasRestModel.Campos obj) =>
                            obj.descricao.Equals(txtLabel.Text.Replace(" *", "")));
                        if (row != null)
                        {
                            var index = Controller.Model.CamposForm.campos.IndexOf(row);
                            var spinner = (Spinner)v;
                            if (spinner.SelectedItemPosition != 0)
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = spinner.SelectedItem.ToString();
                            }
                            else
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = null;
                            }
                        }
                    }
                    //CheckBox
                    else if (forms == 2)
                    {
                        var chck = (CheckBox)v;
                        var row = Controller.Model.CamposForm.campos.FirstOrDefault(
                            (SPromoterMobile.Models.RESTful.FormSchemasRestModel.Campos obj) =>
                            obj.descricao.Equals(chck.Text.Replace(" *", "")));
                        if (row != null)
                        {
                            var index = Controller.Model.CamposForm.campos.IndexOf(row);
                            if (chck.Checked)
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = bool.TrueString;
                            }
                            else
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = bool.FalseString;
                            }
                        }
                    }
                    //EditText
                    else if (forms != 99)
                    {
                        txtLabel = (TextView)layout.GetChildAt(i - 1);
                        var row = Controller.Model.CamposForm.campos.FirstOrDefault(
                            (SPromoterMobile.Models.RESTful.FormSchemasRestModel.Campos obj) =>
                            obj.descricao.Equals(txtLabel.Text.Replace(" *", "")));
                        if (row != null)
                        {
                            var index = Controller.Model.CamposForm.campos.IndexOf(row);
                            edittext = (EditText)v;
                            if (forms == 1)
                            {
                                double outValue = 0;
                                if (!string.IsNullOrEmpty(edittext.Text))
                                {
                                    if (edittext.Text[0] == '.' || edittext.Text[0] == ',')
                                    {
                                        edittext.Text = "0" + edittext.Text;
                                    }
                                    if (double.TryParse(edittext.Text, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out outValue))
                                    {
                                        edittext.Text = outValue.ToString().Replace(",", ".");
                                    }
                                    else
                                    {
                                        edittext.Text = "";
                                    }
                                }
                            }
                            Controller.Model.CamposForm.campos[index].conteudo = edittext.Text;
                        }
                    }
                }
                catch (NullReferenceException) { continue; }
                catch (InvalidCastException) { continue; }
            }
        }

        internal bool HasInvalidField()
        {
            UpdateValues();
            foreach (var campo in Controller.Model.CamposForm.campos)
            {
                var conteudoString = campo.conteudo;
                if ((campo.conteudo == null && campo.minCaracteres > 0))
                {
                    return true;
                }
                if (campo.conteudo != null && conteudoString.Length < campo.minCaracteres)
                {
                    return true;
                }
            }
            return false;
        }

        void IniLabel(TextView txt)
        {
            txt.SetTextSize(ComplexUnitType.Sp, 20);
            txt.SetTextColor(ContextCompat.GetColorStateList(context, Resource.Color.verdeazulescuro));
            layout.AddView(txt);
        }

        void IniEditText(EditText editText, TextView txtDescription,
                         string descricao, int minCaract, int maxCaract)
        {
            editText.SetTextColor(ContextCompat.GetColorStateList(context, Resource.Color.black));
            editText.SetHintTextColor(ContextCompat.GetColorStateList(context, Resource.Color.black));
            editText.BackgroundTintList = ContextCompat.GetColorStateList(context, Resource.Color.black);
            editText.ImeOptions = ImeAction.Next;
            editText.SetTextSize(ComplexUnitType.Sp, 20);
            editText.SetEms(10);
            txtDescription.Text = descricao;
            if (maxCaract > 0)
            {
                IInputFilter[] filterArray = new IInputFilter[1];
                filterArray[0] = new InputFilterLengthFilter(maxCaract);
                editText.SetFilters(filterArray);
            }
            if (minCaract > 0)
            {
                txtDescription.Text += " *";
                editText.OnFocusChangeListener = new OnFocusChange(() =>
                {
                    if (minCaract > editText.Text.Length && editText.Text.Length > 0)
                    {
                        editText.SetTextColor(ContextCompat.GetColorStateList(context,
                                                                              Resource.Color.red));

                        editText.SetHintTextColor(ContextCompat.GetColorStateList(context,
                                        Resource.Color.red));
                    }
                    else
                    {
                        editText.SetTextColor(ContextCompat.GetColorStateList(context,
                                                                              Resource.Color.black));

                        editText.SetHintTextColor(ContextCompat.GetColorStateList(context,
                                                                                  Resource.Color.black));
                    }
                });
            }
        }

        #region EventsListeners
        //Adaptacao tosca e obrigatoria durante a migracao android para monodroid
        sealed class OnFocusChange : Java.Lang.Object, View.IOnFocusChangeListener
        {
            readonly Action action;
            public OnFocusChange(Action action) { this.action = action; }
            void View.IOnFocusChangeListener.OnFocusChange(View v, bool hasFocus)
            { if (!hasFocus) action(); }
        }

        sealed class OnDateSetListener : Java.Lang.Object, DatePickerDialog.IOnDateSetListener
        {
            readonly EditText textDate;
            public OnDateSetListener(EditText textDate) { this.textDate = textDate; }

            public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
            {
                view.UpdateDate(year, monthOfYear, dayOfMonth);
                textDate.Text = view.DateTime.ToString("dd/MM/yyyy");
            }
        }
        #endregion EventsListener


        void ShowPopUpSeekBar(int indexCurrentValueSeekBar)
        {

            var CurrentValueSeekBar = (EditText)layout.GetChildAt(indexCurrentValueSeekBar);
            var dialogView = context.LayoutInflater.Inflate(Resource.Layout.item_popup_seek, null);
            var seekBar = dialogView.FindViewById<SeekBar>(Resource.Id.seekBarPopUp);
            var txtSeekBar = dialogView.FindViewById<TextView>(Resource.Id.txtSeekBar);
            if (int.TryParse(CurrentValueSeekBar.Text, out int valueSeekBar))
            {
                txtSeekBar.Text = CurrentValueSeekBar.Text;
            }
            else
            {
                txtSeekBar.Text = "0%";
            }
            seekBar.Progress = valueSeekBar;
            seekBar.ProgressChanged += (sender, e) =>
            {
                txtSeekBar.Text = seekBar.Progress + "%";
            };

            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(context, Resource.Style.DialogTheme);
            //dialogBuilder.SetTitle(context.Resources.GetString(Resource.String.justificativa));
            dialogBuilder.SetPositiveButton(context.Resources.GetString(Resource.String.ok),
                delegate
                {
                    CurrentValueSeekBar.Text = seekBar.Progress.ToString();
                });
            dialogBuilder.SetView(dialogView);
            context.RunOnUiThread(() => dialogBuilder.Create().Show());
        }

    }
}

