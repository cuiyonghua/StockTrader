﻿/*
 * This library is part of Stock Trader System
 *
 * Copyright (c) qiujoe (http://www.github.com/qiujoe)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * For further information about StockTrader, please see the
 * project website: http://www.github.com/qiujoe/StockTrader
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stock.Strategy;
using Stock.Common;
using Stock.Market;
using System.Collections;
using Microsoft.Scripting.Hosting;
using Stock.Trader;
using System.Windows.Forms;
using Stock.Strategy.Python;
using Stock.Trader.Settings;

namespace Stock.Strategy
{
    public abstract class BaseStrategy : IStrategy, IStockTrader, IControlOperation
    {
        private bool isValid;
        protected ICollection<string> stockPool = new List<string>();
        private IDictionary<String, BidCacheQueue> bids;

        protected StrategyControl control;

        private IDictionary<string, EntrustInfo> entrustNos = new Dictionary<string, EntrustInfo>();
        private class EntrustInfo
        {
            public string No;
            public DateTime Time;
        }

        private void AddEntrustNo(string entrustNo)
        {

            entrustNos.Add(entrustNo, new EntrustInfo { No = entrustNo, Time = DateTime.Now});
        }

        private void RemoveEntrustNo(string entrustNo)
        {
            entrustNos.Remove(entrustNo);
        }

        public StrategyControl Control
        {
            get { return this.control; }
            set { this.control = value; }
        }


        #region 实现策略描述接口

        public event StockRemoveHandler OnStockRemove;
        public event StockAddHandler OnStockAdd;

        public virtual void Run()
        {
            // 检测挂单是否成交，指定时间内未成交撤单
            DateTime now = DateTime.Now;
            int span = int.Parse(Configure.GetStockTraderItem(Configure.CANCEL_TIME_SPAN));

            foreach (EntrustInfo item in entrustNos.Values)
            {
                CancelStock(item.No);
            }
        }

        public virtual void OnStockDataChanged(object sender, Stock.Market.Bid data)
        {
            // NOTHING TO DO
        }

        public abstract void OnTicket(object sender);


        public void AddStock(string code)
        {
            if (OnStockAdd != null)
                OnStockAdd(this, code);

            if(!stockPool.Contains(code))
                stockPool.Add(code);
        }

        public void RemoveStock(string code)
        {
            if (OnStockRemove != null)
                OnStockRemove(this, code);

            stockPool.Remove(code);
        }

        public abstract string Name
        {
            get;
            set;
        }

        public abstract string Description
        {
            get;
            set;
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
            set
            {
                this.isValid = value;
            }
        }

        public String[] StockPool
        {
            get
            {
                return this.stockPool.ToArray<String>();
            }
        }

        #endregion


        public BaseStrategy()
        {
            control = CreateControl();
            this.Init();
        }

        protected virtual StrategyControl CreateControl()
        {
            return new StrategyControl(this);
        }

        /// <summary>
        /// 股票池中的盘口数据
        /// </summary>
        public IDictionary<String, BidCacheQueue> Bids
        {
            get
            {
                if (bids == null)
                {
                    bids = new Dictionary<String, BidCacheQueue>();
                    
                    foreach (string code in StockMarketManager.bidCache.Keys)
	                {
		                if(stockPool.Contains(code)) {
                            bids.Add(code, StockMarketManager.bidCache[code]);
                        }
	                }
                }
                return bids;
            }
        }

        #region 交易接口的实现

        IStockTrader trader = StockTraderManager.Instance.GetStockTrader();

        public virtual void Init()
        {
            // trader.Init();
        }

        public string SellStock(string code, float price, int num)
        {
            LogHelper.WriteLog(this.GetType(),"BaseStrategy.SellStock");
            string entrustNo = trader.SellStock(code, price, num);
            AddEntrustNo(entrustNo);
            return entrustNo;
        }

        public string BuyStock(string code, float price, int num)
        {
            LogHelper.WriteLog(this.GetType(), "BaseStrategy.BuyStock");
            string entrustNo = trader.BuyStock(code, price, num);
            AddEntrustNo(entrustNo);
            return entrustNo;
        }

        public string CancelStock(string entrustNo)
        {
            LogHelper.WriteLog(this.GetType(), "BaseStrategy.CancelStock");
            string eNo = trader.CancelStock(entrustNo);
            RemoveEntrustNo(entrustNo);
            return eNo;
        }

        public void GetTransactionInfo()
        {
            LogHelper.WriteLog(this.GetType(), "BaseStrategy.GetTransactionInfo");
        }

        public void Keep()
        {
            LogHelper.WriteLog(this.GetType(), "BaseStrategy.Keep");
            trader.Keep();
        }

        public TradingAccount GetTradingAccountInfo()
        {
            return null;
        }

        public string PurchaseFundSZ(string code, float total)
        {
            throw new NotImplementedException();
        }

        public string RedempteFundSZ(string code, int num)
        {
            throw new NotImplementedException();
        }

        public string MergeFundSZ(string code, int num)
        {
            throw new NotImplementedException();
        }

        public string PartFundSZ(string code, int num)
        {
            throw new NotImplementedException();
        }

        public string PurchaseFundSH(string code, float total)
        {
            throw new NotImplementedException();
        }

        public string RedempteFundSH(string code, int num)
        {
            throw new NotImplementedException();
        }

        public string MergeFundSH(string code, int num)
        {
            throw new NotImplementedException();
        }

        public string PartFundSH(string code, int num)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region 控件操作接口

        public virtual void Setup()
        {
            // nothing to do
        }

        public virtual void ShowData()
        {
            // thing to do
        }

        public virtual void ImportPool()
        {
            // 
        }

        public virtual IList<StockData> LoadData()
        {
            // nothing to do
            return null;
        }

        #endregion
    }
}