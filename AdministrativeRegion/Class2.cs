//// **************************************************
//// Custom code for ReceiptEntryForm
//// Created: 2018\5\10 星期四 16:08:24
//// **************************************************

//extern alias Erp_Contracts_BO_Receipt;
//extern alias Erp_Contracts_BO_ICReceiptSearch;
//extern alias Erp_Contracts_BO_SupplierXRef;
//extern alias Erp_Contracts_BO_Currency;
//extern alias Erp_Contracts_BO_Company;
//extern alias Erp_Contracts_BO_Part;
//extern alias Erp_Contracts_BO_Vendor;
//extern alias Erp_Contracts_BO_VendorPPSearch;
//extern alias Erp_Contracts_BO_JobEntry;
//extern alias Erp_Contracts_BO_JobAsmSearch;

//using System;
//using System.ComponentModel;
//using System.Data;
//using System.Diagnostics;
//using System.Windows.Forms;
//using Erp.Adapters;
//using Erp.UI;
//using Ice.Lib;
//using Ice.Adapters;
//using Ice.Lib.Customization;
//using Ice.Lib.ExtendedProps;
//using Ice.Lib.Framework;
//using Ice.Lib.Searches;
//using Ice.UI.FormFunctions;
//using Infragistics.Win.UltraWinToolbars;

//public class Script
//{
//    // ** Wizard Insert Location - Do Not Remove 'Begin/End Wizard Added Module Level Variables' Comments! **
//    // Begin Wizard Added Module Level Variables **
//    private EpiDataView edvRcvDtl;
//    private EpiDataView edvRcvHead;



//    private EpiDataView edvMultiKeySearch;
//    // End Wizard Added Module Level Variables **

//    // Add Custom Module Level Variables Here **

//    public void InitializeCustomCode()
//    {
//        // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Variable Initialization' lines **
//        // Begin Wizard Added Variable Initialization
//        this.edvRcvDtl = ((EpiDataView)(this.oTrans.EpiDataViews["RcvDtl"]));
//        this.edvRcvHead = ((EpiDataView)(this.oTrans.EpiDataViews["RcvHead"]));

//        this.edvRcvDtl.dataView.Table.Columns["Received"].ExtendedProperties["ReadOnly"] = true;
//        this.edvRcvHead.dataView.Table.Columns["Received"].ExtendedProperties["ReadOnly"] = true;

//        //this.edvRcvDtl.EpiViewNotification += new EpiViewNotification(this.edvRcvDtl_EpiViewNotification);
//        this.edvRcvHead.EpiViewNotification += new EpiViewNotification(this.edvRcvHead_EpiViewNotification);

//        this.edvMultiKeySearch = ((EpiDataView)(this.oTrans.EpiDataViews["MultiKeySearch"]));
//        this.edvMultiKeySearch.EpiViewNotification += new EpiViewNotification(this.edvMultiKeySearch_EpiViewNotification);
//        this.RcvHead_Column.ColumnChanged += new DataColumnChangeEventHandler(this.RcvHead_AfterFieldChange);
//        //this.RcvHead_Column.ColumnChanging -= new DataColumnChangeEventHandler(this.RcvHead_BeforeFieldChange);
//        this.ReceiptEntryForm.BeforeToolClick += new Ice.Lib.Framework.BeforeToolClickEventHandler(this.ReceiptEntryForm_BeforeToolClick);
//        // End Wizard Added Variable Initialization

//        // Begin Wizard Added Custom Method Calls

//        // End Wizard Added Custom Method Calls
//    }
//    private void ReceiptEntryForm_BeforeToolClick(object sender, Ice.Lib.Framework.BeforeToolClickEventArgs args)
//    {
//        switch (args.Tool.Key)
//        {
//            case "NewMenuTool":
//                if (oTrans.LastView.ViewName != "RcvHead")
//                {
//                    args.Handled = true;
//                }
//                break;
//        }
//    }
//    private void RcvHead_BeforeFieldChange(object sender, DataColumnChangeEventArgs args)
//    {
//        // ** Argument Properties and Uses **
//        // args.Row["FieldName"]
//        // args.Column, args.ProposedValue, args.Row
//        // Add Event Handler Code
//        switch (args.Column.ColumnName)
//        {
//            case "Received_c":
//                bool status = Convert.ToBoolean(args.ProposedValue);
//                if (status == false)
//                {
//                    args.Row.CancelEdit();
//                    args.ProposedValue = args.Row[args.Column.ColumnName];
//                    EpiMessageBox.Show("不可以取消，因为有收货行需要品检！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//                else if (AllConf() == false)
//                {
//                    args.Row["Received"] = status;
//                }
//                break;
//        }
//    }
//    private bool AllConf()
//    {
//        bool revalue = false;
//        DataRow[] drs = this.edvRcvDtl.dataView.Table.Select("InspectionReq =true");
//        if (drs.Length > 0)
//        {
//            revalue = true;
//            int rows = this.edvRcvDtl.dataView.Count - 1;
//            for (int i = rows; i >= 0; i--)
//            {
//                if (Convert.ToBoolean(this.edvRcvDtl.dataView[i]["InspectionReq"]) == false
//                    && Convert.ToBoolean(this.edvRcvDtl.dataView[i]["Received"]) == false)
//                {
//                    this.edvRcvDtl.Row = i;
//                    this.edvRcvDtl.dataView[i].BeginEdit();
//                    this.edvRcvDtl.dataView[i]["Received"] = true;
//                    this.edvRcvDtl.dataView[i].EndEdit();
//                }
//            }
//        }
//        return revalue;
//    }
//    private void edvRcvHead_EpiViewNotification(EpiDataView view, EpiNotifyArgs args)
//    {
//        // ** Argument Properties and Uses **
//        // view.dataView[args.Row]["FieldName"]
//        // args.Row, args.Column, args.Sender, args.NotifyType
//        // NotifyType.Initialize, NotifyType.AddRow, NotifyType.DeleteRow, NotifyType.InitLastView, NotifyType.InitAndResetTreeNodes
//        if ((args.NotifyType == EpiTransaction.NotifyType.Initialize))
//        {
//            if ((args.Row > -1))
//            {
//                DisableControls(ReceiptEntryForm, (bool)view.dataView[args.Row]["Received_c"]);
//                EnabledTools((bool)view.dataView[args.Row]["Received_c"]);
//            }
//            else
//            {
//                DisableControls(ReceiptEntryForm, true);
//                EnabledTools(false);
//            }
//        }
//    }
//    private void DisableControls(Control Container, bool status)
//    {
//        try
//        {
//            if (Container.GetType() != typeof(UltraToolbarsDockArea)
//                && Container.GetType() != typeof(Infragistics.Win.UltraWinStatusBar.UltraStatusBar)
//                && Container.GetType() != typeof(EpiTreeViewPanel)
//                && Container.GetType() != typeof(EpiUltraComboPlus))
//            {
//                IDisabled disabled = Container as IDisabled;
//                if (disabled != null)
//                {
//                    disabled.Disabled = status;
//                    if (!status)
//                        Container.TabStop = true;
//                }
//            }
//        }
//        catch (Exception exception)
//        {
//            TraceProvider.TraceException(exception);
//        }
//        foreach (Control control in Container.Controls)
//        {
//            try
//            {
//                if (control.Name != "gbStatus")
//                {
//                    if (control.GetType() != typeof(UltraToolbarsDockArea)
//                        && control.GetType() != typeof(Infragistics.Win.UltraWinStatusBar.UltraStatusBar)
//                        && control.GetType() != typeof(EpiTreeViewPanel)
//                        && control.GetType() != typeof(EpiUltraComboPlus))
//                    {
//                        IDisabled disabled = control as IDisabled;
//                        if (disabled != null)
//                        {
//                            disabled.Disabled = status;
//                            if (!status)
//                            {
//                                control.TabStop = true;
//                            }
//                        }
//                        else if (control.Controls.Count > 0)
//                        {
//                            DisableControls(control, status);
//                        }
//                    }
//                }
//            }
//            catch (Exception exception)
//            {
//                TraceProvider.TraceException(exception);
//                continue;
//            }
//        }
//    }
//    private void EnabledTools(bool status)
//    {
//        baseToolbarsManager.Tools["DeleteTool"].SharedProps.Visible = !status;
//        baseToolbarsManager.Tools["CreateMassReceiptTool"].SharedProps.Visible = !status;

//        PopupMenuTool tool = (PopupMenuTool)this.baseToolbarsManager.Tools["NewMenuTool"];
//        int n = tool.Tools.Count;
//        for (int i = 0; i < n; i++)
//        {
//            if (tool.Tools[i].Key != "EpiAddNewNewReceipt")
//            {
//                tool.Tools[i].SharedProps.Visible = !status;
//            }
//        }
//    }

//    public void DestroyCustomCode()
//    {
//        // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
//        // Begin Wizard Added Object Disposal

//        this.edvMultiKeySearch.EpiViewNotification -= new EpiViewNotification(this.edvMultiKeySearch_EpiViewNotification);
//        this.edvMultiKeySearch = null;
//        this.RcvHead_Column.ColumnChanged -= new DataColumnChangeEventHandler(this.RcvHead_AfterFieldChange);
//        // End Wizard Added Object Disposal

//        // Begin Custom Code Disposal

//        // End Custom Code Disposal
//    }

//    private void edvMultiKeySearch_EpiViewNotification(EpiDataView view, EpiNotifyArgs args)
//    {
//        // ** Argument Properties and Uses **
//        // view.dataView[args.Row]["FieldName"]
//        // args.Row, args.Column, args.Sender, args.NotifyType
//        // NotifyType.Initialize, NotifyType.AddRow, NotifyType.DeleteRow, NotifyType.InitLastView, NotifyType.InitAndResetTreeNodes
//        if ((args.NotifyType == EpiTransaction.NotifyType.AddRow))
//        {
//            if ((args.Row > -1))
//            {
//            }
//        }
//    }

//    private void RcvHead_AfterFieldChange(object sender, DataColumnChangeEventArgs args)
//    {
//        // ** Argument Properties and Uses **
//        // args.Row["FieldName"]
//        // args.Column, args.ProposedValue, args.Row
//        // Add Event Handler Code
//        switch (args.Column.ColumnName)
//        {

//            case "VendorNumVendorID":
//                AutoSetZXDNo();
//                break;
//            case "PONum":
//                AutoSetZXDNo();
//                break;
//        }
//    }
//    public void AutoSetZXDNo()
//    {
//        Ice.Lib.Framework.EpiTextBox ZXDText;
//        ZXDText = (Ice.Lib.Framework.EpiTextBox)(csm.GetNativeControlReference("17fb79b9-2a5d-474a-b9d1-5e5233a16cde"));
//        string zxd;
//        //TimeSpan ts = DateTime.UtcNow;	
//        // TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
//        zxd = string.Format("{0:yyyyMMddHHmmssffff}", DateTime.UtcNow);
//        //zxd = Convert.ToInt64(ts.TotalSeconds).ToString();
//        edvMultiKeySearch = ((EpiDataView)(oTrans.EpiDataViews["MultiKeySearch"]));
//        edvMultiKeySearch.dataView[edvMultiKeySearch.Row]["PackSlip"] = zxd;
//    }
//}




