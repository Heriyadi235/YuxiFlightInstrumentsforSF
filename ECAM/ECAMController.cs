using Assets.YuxiFlightInstruments.ECAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Assets.YuxiFlightInstruments.ECAM {
    public partial class ECAMController : UdonSharpBehaviour {
        public ChecklistItem[] Checklists;
        public ChecklistItem[] ActiveChecklists = new ChecklistItem[0];

        public Text LeftMemoText;
        public Text RightMemoText;

        [UdonSynced, FieldChangeCallback(nameof(IsLadingGearDown))]
        private bool _isLadingGearDown;
        public bool IsLadingGearDown {
            get => _isLadingGearDown;
            set {
                _isLadingGearDown = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsSeatBeltsSignOn))]
        private bool _isSeatBeltsSignOn;
        public bool IsSeatBeltsSignOn {
            get => _isSeatBeltsSignOn;
            set {
                _isSeatBeltsSignOn = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsNoSmoking))]
        private bool _isNoSmoking;
        public bool IsNoSmoking {
            get => _isNoSmoking;
            set {
                _isNoSmoking = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsSplrsArmed))]
        private bool _isSplrsArmed;
        public bool IsSplrsArmed {
            get => _isSplrsArmed;
            set {
                _isSplrsArmed = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsFlapOut))]
        private bool _isFlapOut;
        public bool IsFlapOut {
            get => _isFlapOut;
            set {
                _isFlapOut = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsCabinReady))]
        private bool _isCabinReady;
        public bool IsCabinReady {
            get => _isCabinReady;
            set {
                _isCabinReady = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsParkbrakeSet))]
        private bool _isParkbrakeSet;
        public bool IsParkbrakeSet {
            get => _isParkbrakeSet;
            set {
                _isParkbrakeSet = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsHookDown))]
        private bool _isHookDown;
        public bool IsHookDown {
            get => _isHookDown;
            set {
                _isHookDown = value;
                UpdateECAMMemo();
            }
        }

        public void Start() {
            IsParkbrakeSet = false;
            IsSeatBeltsSignOn = true;

            ActiveChecklists = new ChecklistItem[] {
                Checklists[1]
            };
            
            UpdateChecklist();
        }

        public void UpdateChecklist() {
            LeftMemoText.text = "";
            foreach (var item in ActiveChecklists) {
                var hasTitle = !string.IsNullOrEmpty(item.Title);

                if (hasTitle) {
                    LeftMemoText.text += $"{item.Prefix} {item.Title}\n";
                } else {
                    LeftMemoText.text += $"{item.Prefix} ";
                }

                var prefix = "".PadLeft((item.Prefix + " ").Length);

                for (int index = 0; index != item.CheckItems.Length; index++) {
                    var checkitem = item.CheckItems[index];
                    var checkItemTextLength = 20 + $"<color={MemoItemColor.Blue}>".Length - checkitem.ValueText.Length;
                    var isChecked = GetProgramVariable(checkitem.PropertyName).ToString() == checkitem.Value;
                    var checkItemText = "";

                    if (!hasTitle && index == 0) {
                        checkItemTextLength -= (item.Prefix + " ").Length;
                    } else {
                        checkItemText = prefix;
                    }

                    if (isChecked) {
                        checkItemText += $"{checkitem.Title} {checkitem.ValueText}\n";
                    } else {
                        checkItemText += $"{checkitem.Title}<color={ MemoItemColor.Blue }>";
                        checkItemText = checkItemText.PadRight(checkItemTextLength, '.');
                        checkItemText += $"{checkitem.ValueText}</color>\n";
                    }

                    LeftMemoText.text += checkItemText;
                }
            }
        }

        public void UpdateECAMMemo() {
            UpdateChecklist();
            //UpdateLeftMemo();
            UpdateRightMemo();
        }

        public void UpdateLeftMemo() {
            var leftMemo = "";
            if (IsSplrsArmed) leftMemo += CreateECAMMemo(MemoItemColor.Green, "GND SPLRS ARMED");
            if (IsSeatBeltsSignOn) leftMemo += CreateECAMMemo(MemoItemColor.Green, "SEAT BELTS");
            if (IsNoSmoking) leftMemo += CreateECAMMemo(MemoItemColor.Green, "NO SMOKING");

            LeftMemoText.text = leftMemo;
        }

        public void UpdateRightMemo() {
            var rightMemo = "";
            if (IsParkbrakeSet) rightMemo += CreateECAMMemo(MemoItemColor.Green, "PARK BRAKE");
            if (IsCabinReady) rightMemo += CreateECAMMemo(MemoItemColor.Green, "CABIN READY");
            if (IsHookDown) rightMemo += CreateECAMMemo(MemoItemColor.Green, "HOOK");
            RightMemoText.text = rightMemo;
        }

        public string CreateECAMMemo(string color, string text) {
            return $"<color={color}>{text}</color>\n";
        }
    }
}
