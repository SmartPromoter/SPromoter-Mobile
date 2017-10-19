//
//  UIFormDinamico.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using CoreGraphics;
using SPromoterMobile;
using UIKit;
using HockeyApp;

namespace SmartPromoter.Iphone
{
    public class UIFormDinamico
    {
        FormDinamicoCon Controller { get; }
        public FormDinamicoController UiView { get; }
        readonly UIScrollView scrollViewContent;
        readonly string datePickerString = "Clique aqui para inserir uma data";
        readonly string percentString = "Clique aqui para inserir um percentual";
        readonly string spinnerString = "Selecione...";
        readonly nfloat size = 50;

        public UIFormDinamico(FormDinamicoCon controller, FormDinamicoController UiView, UIScrollView scrollViewContent)
        {
            try
            {
                Controller = controller;
                this.UiView = UiView;
                this.scrollViewContent = scrollViewContent;
                this.scrollViewContent.Bounds = UIScreen.MainScreen.Bounds;
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Referencias do UIFormDinamico null");
            }
        }

        public void IniForm()
        {
            nfloat frame = 10;
            for (int item = 0; item <= Controller.Model.CamposForm.campos.Count - 1; item++)
            {
                var forms = Controller.Model.CamposForm.campos[item].tipo;
                var minCaract = Controller.Model.CamposForm.campos[item].minCaracteres;
                var maxCaract = Controller.Model.CamposForm.campos[item].maxCaracteres;
                switch (forms)
                {
                    case (0):
                        frame = CreateStringField(Controller.Model.CamposForm.campos[item].descricao, frame, size,
                                                  Controller.Model.CamposForm.campos[item].conteudo, maxCaract, minCaract);
                        break;
                    case (1):
                        frame = CreateNumberField(Controller.Model.CamposForm.campos[item].descricao, frame, size,
                                                  Controller.Model.CamposForm.campos[item].conteudo, maxCaract, minCaract);
                        break;
                    case (2):
                        if (!Controller.Model.CamposForm.campos[item].descricao.ToUpper().Equals("RUPTURA"))
                        {
                            if (bool.TryParse(Controller.Model.CamposForm.campos[item].conteudo, out bool checkbox))
                            {
                                frame = CreateSwitchField(Controller.Model.CamposForm.campos[item].descricao, frame, size, checkbox);
                            }
                            else
                            {
                                frame = CreateSwitchField(Controller.Model.CamposForm.campos[item].descricao, frame, size, false);
                            }
                        }
                        break;
                    case (3):
                        frame = CreateDatePickerField(Controller.Model.CamposForm.campos[item].descricao, frame, size,
                                                      Controller.Model.CamposForm.campos[item].conteudo, minCaract);
                        break;
                    case (4):
                        frame = CreateSliderField(Controller.Model.CamposForm.campos[item].descricao, frame, size,
                                                  Controller.Model.CamposForm.campos[item].conteudo, minCaract);
                        break;
                    case (5):
                        var spinnerArray = new List<string>();
                        foreach (var spinnerItem in Controller.Model.CamposForm.campos[item].opcoes)
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
                        frame = CreateSpinnerField(Controller.Model.CamposForm.campos[item].descricao, frame, size,
                                                   Controller.Model.CamposForm.campos[item].conteudo, spinnerArray, minCaract);
                        break;
                    default:
                        throw new InvalidCastException("Tipo nao reconhecido no formulário");
                }
            }
            scrollViewContent.ContentSize = new CGSize(50, frame + 160);
            scrollViewContent.ContentMode = UIViewContentMode.TopLeft;
        }

        internal void UpdateValues()
        {
            for (var i = 0; i < scrollViewContent.Subviews.Length; i++)
            {
                UIView v = scrollViewContent.Subviews[i];
                var forms = (int)v.Tag;
                if (forms > 0)
                {
                    var label = (UILabel)scrollViewContent.Subviews[i - 1];
                    var row = Controller.Model.CamposForm.campos.FirstOrDefault(
                            (SPromoterMobile.Models.RESTful.FormSchemasRestModel.Campos obj) =>
                                obj.descricao.Equals(label.Text.Replace(" *", "")));
                    if (row != null)
                    {
                        var index = Controller.Model.CamposForm.campos.IndexOf(row);
                        if (forms == 1)
                        {
                            var textView = (UITextView)v;
                            if (string.IsNullOrEmpty(textView.Text))
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = null;
                            }
                            else
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = textView.Text;
                            }
                        }
                        else if (forms == 2)
                        {
                            var textView = (UITextView)v;
                            double outValue = 0;
                            if (!string.IsNullOrEmpty(textView.Text))
                            {
                                if (textView.Text[0] == '.' || textView.Text[0] == ',')
                                {
                                    textView.Text = "0" + textView.Text;
                                }
                                if (double.TryParse(textView.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out outValue))
                                {
                                    textView.Text = outValue.ToString().Replace(",", ".");
                                }
                                else
                                {
                                    textView.Text = "";
                                }
                            }
                            Controller.Model.CamposForm.campos[index].conteudo = textView.Text;
                        }
                        else if (forms == 3)
                        {
                            var swithView = (UISwitch)v;
                            Controller.Model.CamposForm.campos[index].conteudo = swithView.On.ToString();
                        }
                        else if (forms == 4 || forms == 5 || forms == 6)
                        {
                            var buttonView = (UIButton)v;
                            if (buttonView.CurrentTitle.Contains(datePickerString) ||
                                buttonView.CurrentTitle.Contains(spinnerString) ||
                                buttonView.CurrentTitle.Contains(percentString))
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = null;
                            }
                            else
                            {
                                Controller.Model.CamposForm.campos[index].conteudo = buttonView.CurrentTitle;
                            }
                        }
                    }
                }
            }
        }

        internal bool HasInvalidateFields()
        {
            UpdateValues();
            foreach (var campo in Controller.Model.CamposForm.campos)
            {
                if ((campo.conteudo == null && campo.minCaracteres > 0) || (campo.conteudo != null && campo.conteudo.Length < campo.minCaracteres))
                {
                    return true;
                }

            }
            return false;
        }

        #region EventHandlersPopUp
        EventHandler SliderPoPUp(UIButton btnLabel)
        {
            return (sender, e) =>
            {
                var swithView = new UISlider
                {
                    Frame = new CGRect(10, 40, 250, 60),
                    TintColor = UIColor.FromRGB(10, 88, 90)
                };
                if (!string.IsNullOrEmpty(btnLabel.CurrentTitle))
                {
                    if (int.TryParse(btnLabel.CurrentTitle.Replace("%", "").Trim(), out int currentValue))
                    {
                        swithView.SetValue(currentValue / 100, true);
                    }
                }
                var alert = UIAlertController.Create(btnLabel.CurrentTitle, "\n\n", UIAlertControllerStyle.Alert);
                alert.Add(swithView);
                alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
                {
                    MetricsManager.TrackEvent("CancelFormDinamico");
                }));
                alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
                {
                    var result = (int)(swithView.Value * 100);
                    btnLabel.SetTitle(result.ToString(), UIControlState.Normal);
                }));
                alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                UiView.PresentViewController(alert, true, null);
                swithView.ValueChanged += delegate
                {
                    alert.Title = swithView.Value * 100 + "%";
                };
            };
        }

        EventHandler SpinnerPopUp(List<string> dropDownListItem, UIButton btnLabel)
        {
            return (sender, e) =>
            {
                var actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
                foreach (string item in dropDownListItem)
                {
                    actionSheetAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, (action) => { btnLabel.SetTitle(item, UIControlState.Normal); }));
                }
                actionSheetAlert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, (action) => { }));
                var presentationPopover = actionSheetAlert.PopoverPresentationController;
                if (presentationPopover != null)
                {
                    presentationPopover.SourceView = UiView.View;
                    presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                }
                actionSheetAlert.View.TintColor = UIColor.Black;
                UiView.PresentViewController(actionSheetAlert, true, null);
            };
        }

        EventHandler DatePickerPopUp(UIButton btnLabel)
        {
            return (sender, e) =>
            {
                var swithView = new UIDatePicker
                {
                    Mode = UIDatePickerMode.Date,
                    Frame = new CGRect(10, 5, 250, 160)
                };
                var alert = UIAlertController.Create("\n\n\n", "\n\n\n", UIAlertControllerStyle.Alert);
                alert.Add(swithView);
                alert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, (actionCancel) =>
                {
                    MetricsManager.TrackEvent("CancelDatePicker");
                }));
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (actionOK) =>
                {
                    var date = (DateTime)swithView.Date;
                    btnLabel.SetTitle(date.ToString("d"), UIControlState.Normal);
                }));
                alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                UiView.PresentViewController(alert, true, null);
            };
        }
        #endregion EventHandlersPopUp

        #region CreateFields
        nfloat LabelField(string descriptionLabel, nfloat yFrame)
        {

            var label = new UILabel(new CGRect(10, yFrame, UiView.View.Bounds.Size.Width - 10, 25))
            {
                Text = descriptionLabel,
                Font = UIFont.FromName("Helvetica-Bold", 16f)
            };
            scrollViewContent.Add(label);
            return yFrame + 25;
        }

        nfloat CreateStringField(string descriptionLabel, nfloat yFrame, nfloat sizeFrame, object conteudo, int maxCaracteres, int minCaracteres)
        {
            var conteudoString = (string)conteudo;
            if (minCaracteres > 0)
            {
                yFrame = LabelField(descriptionLabel + " *", yFrame);
            }
            else
            {
                yFrame = LabelField(descriptionLabel, yFrame);
            }

            var textField = new UITextView(new CGRect(10, yFrame, scrollViewContent.Bounds.Size.Width - 20, sizeFrame))
            {
                KeyboardType = UIKeyboardType.Default,
                ReturnKeyType = UIReturnKeyType.Next
            };
            if (!string.IsNullOrEmpty(conteudoString))
            {
                textField.Text = conteudoString;
            }
            textField.ShouldChangeText += (textView, range, text) =>
            {
                if (text.Equals("\n"))
                {
                    textField.EndEditing(true);
                    return false;
                }
                if (maxCaracteres > 0)
                {
                    var newLength = (textField.Text.Length - range.Length) + text.Length;
                    return newLength <= maxCaracteres;
                }
                return true;
            };

            CGPoint point;
            if (yFrame - 25 < 0)
            {
                point = new CGPoint(0, 0);
            }
            else
            {
                point = new CGPoint(0, yFrame - 25);
            }
            textField.ShouldBeginEditing = (textView) =>
            {
                scrollViewContent.SetContentOffset(point, true);
                return true;
            };

            textField.Tag = 1;
            textField.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            textField.Layer.CornerRadius = 5.0f;
            textField.LayoutMargins = new UIEdgeInsets(0, 20, 0, 20);
            textField.Layer.MasksToBounds = true;

            scrollViewContent.Add(textField);
            return yFrame + sizeFrame;
        }

        nfloat CreateNumberField(string descriptionLabel, nfloat yFrame, nfloat sizeFrame, object conteudo, int maxCaracteres, int minCaracteres)
        {
            var conteudoString = (string)conteudo;
            if (minCaracteres > 0)
            {
                yFrame = LabelField(descriptionLabel + " *", yFrame);
            }
            else
            {
                yFrame = LabelField(descriptionLabel, yFrame);
            }
            var textField = new UITextView(new CGRect(10, yFrame, scrollViewContent.Bounds.Size.Width - 20, sizeFrame))
            {
                KeyboardType = UIKeyboardType.NumbersAndPunctuation,
                ReturnKeyType = UIReturnKeyType.Next
            };
            if (!string.IsNullOrEmpty(conteudoString))
            {
                textField.Text = conteudoString;
            }

            textField.ShouldChangeText += (textView, range, text) =>
            {
                if (text.Equals("\n"))
                {
                    textField.EndEditing(true);
                    return false;
                }

                if (text.Equals(""))
                {
                    return true;
                }

                if (text.Equals(","))
                {
                    text += "0";
                }
                if (text.Equals(".") || !double.TryParse(textView.Text + text, out double auxValor))
                {
                    return false;
                }
                if (maxCaracteres > 0)
                {
                    var newLength = (textField.Text.Length - range.Length) + text.Length;
                    return newLength <= maxCaracteres;
                }
                return true;
            };

            CGPoint point;
            if (yFrame - 20 < 0)
            {
                point = new CGPoint(0, 0);
            }
            else
            {
                point = new CGPoint(0, yFrame - 20);
            }
            textField.ShouldBeginEditing = (textView) =>
            {
                scrollViewContent.SetContentOffset(point, true);
                return true;
            };


            textField.Tag = 2;
            textField.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            textField.Layer.CornerRadius = 5.0f;
            textField.Layer.MasksToBounds = true;
            textField.LayoutMargins = new UIEdgeInsets(0, 20, 0, 20);

            scrollViewContent.Add(textField);
            return yFrame + sizeFrame;
        }

        nfloat CreateSwitchField(string descripionLabel, nfloat yFrame, nfloat sizeFrame, bool conteudo)
        {
            yFrame = LabelField(descripionLabel, yFrame);
            var switchButton = new UISwitch
            {
                On = conteudo,
                Frame = new CGRect(40, yFrame, scrollViewContent.Bounds.Size.Width - 20, sizeFrame),
                Tag = 3
            };
            scrollViewContent.Add(switchButton);
            return yFrame + sizeFrame;
        }

        nfloat CreateDatePickerField(string descriptionLabel, nfloat yFrame, nfloat sizeFrame, object conteudo, int minCaracteres)
        {
            var conteudoString = (string)conteudo;
            if (minCaracteres > 0)
            {
                yFrame = LabelField(descriptionLabel + " *", yFrame);
            }
            else
            {
                yFrame = LabelField(descriptionLabel, yFrame);
            }
            var dtPickButton = UIButton.FromType(UIButtonType.RoundedRect);
            dtPickButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            if (string.IsNullOrEmpty(conteudoString))
            {
                dtPickButton.SetTitle(datePickerString, UIControlState.Normal);
            }
            else
            {
                dtPickButton.SetTitle(conteudoString, UIControlState.Normal);
            }
            dtPickButton.Frame = new CGRect(10, yFrame, scrollViewContent.Bounds.Size.Width - 20, sizeFrame);
            dtPickButton.TouchDown -= DatePickerPopUp(dtPickButton);
            dtPickButton.TouchDown += DatePickerPopUp(dtPickButton);
            dtPickButton.Tag = 4;
            dtPickButton.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            dtPickButton.Layer.CornerRadius = 5.0f;
            dtPickButton.Layer.MasksToBounds = true;
            dtPickButton.TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 5);

            scrollViewContent.Add(dtPickButton);
            return yFrame + sizeFrame;
        }

        nfloat CreateSliderField(string descriptionLabel, nfloat yFrame, nfloat sizeFrame, object conteudo, int minCaracteres)
        {
            var conteudoString = (string)conteudo;
            if (minCaracteres > 0)
            {
                yFrame = LabelField(descriptionLabel + " *", yFrame);
            }
            else
            {
                yFrame = LabelField(descriptionLabel, yFrame);
            }
            var sliderBtn = UIButton.FromType(UIButtonType.RoundedRect);
            sliderBtn.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            if (string.IsNullOrEmpty(conteudoString))
            {
                sliderBtn.SetTitle(percentString, UIControlState.Normal);
            }
            else
            {
                sliderBtn.SetTitle(conteudoString, UIControlState.Normal);
            }
            sliderBtn.Frame = new CGRect(10, yFrame, scrollViewContent.Bounds.Size.Width - 20, sizeFrame);
            sliderBtn.TouchDown -= SliderPoPUp(sliderBtn);
            sliderBtn.TouchDown += SliderPoPUp(sliderBtn);
            sliderBtn.Tag = 5;
            sliderBtn.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            sliderBtn.Layer.CornerRadius = 5.0f;
            sliderBtn.Layer.MasksToBounds = true;
            sliderBtn.TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 5);

            scrollViewContent.Add(sliderBtn);
            return yFrame + sizeFrame;
        }

        nfloat CreateSpinnerField(string descriptionLabel, nfloat yFrame, nfloat sizeFrame, object conteudo, List<string> listSpinner, int minCaracteres)
        {
            var conteudoString = (string)conteudo;
            if (minCaracteres > 0)
            {
                yFrame = LabelField(descriptionLabel + " *", yFrame);
            }
            else
            {
                yFrame = LabelField(descriptionLabel, yFrame);
            }
            var spinner = UIButton.FromType(UIButtonType.RoundedRect);
            spinner.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            if (string.IsNullOrEmpty(conteudoString))
            {
                spinner.SetTitle(spinnerString, UIControlState.Normal);
            }
            else
            {
                spinner.SetTitle(conteudoString, UIControlState.Normal);
            }
            spinner.Frame = new CGRect(10, yFrame, scrollViewContent.Bounds.Size.Width - 20, sizeFrame);
            spinner.TouchDown -= SpinnerPopUp(listSpinner, spinner);
            spinner.TouchDown += SpinnerPopUp(listSpinner, spinner);
            spinner.Tag = 6;
            spinner.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            spinner.Layer.CornerRadius = 5.0f;
            spinner.Layer.MasksToBounds = true;
            spinner.TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 5);

            scrollViewContent.Add(spinner);
            return yFrame + sizeFrame;
        }
        #endregion CreateFields
    }
}
