using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NativeWifi;

public class WlanClient : IDisposable
{
	public class WlanInterface
	{
		public delegate void WlanNotificationEventHandler(Wlan.WlanNotificationData notifyData);

		public delegate void WlanConnectionNotificationEventHandler(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData);

		public delegate void WlanReasonNotificationEventHandler(Wlan.WlanNotificationData notifyData, Wlan.WlanReasonCode reasonCode);

		private struct WlanConnectionNotificationEventData
		{
			public Wlan.WlanNotificationData notifyData;

			public Wlan.WlanConnectionNotificationData connNotifyData;
		}

		private struct WlanReasonNotificationData
		{
			public Wlan.WlanNotificationData notifyData;

			public Wlan.WlanReasonCode reasonCode;
		}

		private readonly WlanClient m_0;

		private Wlan.WlanInterfaceInfo m_0;

		private WlanNotificationEventHandler m_0;

		private WlanConnectionNotificationEventHandler m_0;

		private WlanReasonNotificationEventHandler m_0;

		private bool m_0;

		private readonly AutoResetEvent m_0 = new AutoResetEvent(initialState: false);

		private readonly Queue<object> m_0 = new Queue<object>();

		public bool Autoconf
		{
			get
			{
				return _0(Wlan.WlanIntfOpcode.AutoconfEnabled) != 0;
			}
			set
			{
				_0(Wlan.WlanIntfOpcode.AutoconfEnabled, value ? 1 : 0);
			}
		}

		public Wlan.Dot11BssType BssType
		{
			get
			{
				return (Wlan.Dot11BssType)_0(Wlan.WlanIntfOpcode.BssType);
			}
			set
			{
				_0(Wlan.WlanIntfOpcode.BssType, (int)value);
			}
		}

		public Wlan.WlanInterfaceState InterfaceState => (Wlan.WlanInterfaceState)_0(Wlan.WlanIntfOpcode.InterfaceState);

		public int Channel => _0(Wlan.WlanIntfOpcode.ChannelNumber);

		public int RSSI => _0(Wlan.WlanIntfOpcode.RSSI);

		public Wlan.WlanRadioState RadioState
		{
			get
			{
				Wlan._0(Wlan.WlanQueryInterface(this.m_0.m_0, this.m_0.interfaceGuid, Wlan.WlanIntfOpcode.RadioState, IntPtr.Zero, out var _, out var ppData, out var _));
				try
				{
					return (Wlan.WlanRadioState)Marshal.PtrToStructure(ppData, typeof(Wlan.WlanRadioState));
				}
				finally
				{
					Wlan.WlanFreeMemory(ppData);
				}
			}
		}

		public Wlan.Dot11OperationMode CurrentOperationMode => (Wlan.Dot11OperationMode)_0(Wlan.WlanIntfOpcode.CurrentOperationMode);

		public Wlan.WlanConnectionAttributes CurrentConnection
		{
			get
			{
				Wlan._0(Wlan.WlanQueryInterface(this.m_0.m_0, this.m_0.interfaceGuid, Wlan.WlanIntfOpcode.CurrentConnection, IntPtr.Zero, out var _, out var ppData, out var _));
				try
				{
					return (Wlan.WlanConnectionAttributes)Marshal.PtrToStructure(ppData, typeof(Wlan.WlanConnectionAttributes));
				}
				finally
				{
					Wlan.WlanFreeMemory(ppData);
				}
			}
		}

		public NetworkInterface NetworkInterface
		{
			get
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface networkInterface in allNetworkInterfaces)
				{
					if (new Guid(networkInterface.Id).Equals(this.m_0.interfaceGuid))
					{
						return networkInterface;
					}
				}
				return null;
			}
		}

		public Guid InterfaceGuid => this.m_0.interfaceGuid;

		public string InterfaceDescription => this.m_0.interfaceDescription;

		public string InterfaceName => NetworkInterface.Name;

		public event WlanNotificationEventHandler WlanNotification
		{
			add
			{
				WlanNotificationEventHandler wlanNotificationEventHandler = this.m_0;
				WlanNotificationEventHandler wlanNotificationEventHandler2;
				do
				{
					wlanNotificationEventHandler2 = wlanNotificationEventHandler;
					WlanNotificationEventHandler value2 = (WlanNotificationEventHandler)Delegate.Combine(wlanNotificationEventHandler2, value);
					wlanNotificationEventHandler = Interlocked.CompareExchange(ref this.m_0, value2, wlanNotificationEventHandler2);
				}
				while ((object)wlanNotificationEventHandler != wlanNotificationEventHandler2);
			}
			remove
			{
				WlanNotificationEventHandler wlanNotificationEventHandler = this.m_0;
				WlanNotificationEventHandler wlanNotificationEventHandler2;
				do
				{
					wlanNotificationEventHandler2 = wlanNotificationEventHandler;
					WlanNotificationEventHandler value2 = (WlanNotificationEventHandler)Delegate.Remove(wlanNotificationEventHandler2, value);
					wlanNotificationEventHandler = Interlocked.CompareExchange(ref this.m_0, value2, wlanNotificationEventHandler2);
				}
				while ((object)wlanNotificationEventHandler != wlanNotificationEventHandler2);
			}
		}

		public event WlanConnectionNotificationEventHandler WlanConnectionNotification
		{
			add
			{
				WlanConnectionNotificationEventHandler wlanConnectionNotificationEventHandler = this.m_0;
				WlanConnectionNotificationEventHandler wlanConnectionNotificationEventHandler2;
				do
				{
					wlanConnectionNotificationEventHandler2 = wlanConnectionNotificationEventHandler;
					WlanConnectionNotificationEventHandler value2 = (WlanConnectionNotificationEventHandler)Delegate.Combine(wlanConnectionNotificationEventHandler2, value);
					wlanConnectionNotificationEventHandler = Interlocked.CompareExchange(ref this.m_0, value2, wlanConnectionNotificationEventHandler2);
				}
				while ((object)wlanConnectionNotificationEventHandler != wlanConnectionNotificationEventHandler2);
			}
			remove
			{
				WlanConnectionNotificationEventHandler wlanConnectionNotificationEventHandler = this.m_0;
				WlanConnectionNotificationEventHandler wlanConnectionNotificationEventHandler2;
				do
				{
					wlanConnectionNotificationEventHandler2 = wlanConnectionNotificationEventHandler;
					WlanConnectionNotificationEventHandler value2 = (WlanConnectionNotificationEventHandler)Delegate.Remove(wlanConnectionNotificationEventHandler2, value);
					wlanConnectionNotificationEventHandler = Interlocked.CompareExchange(ref this.m_0, value2, wlanConnectionNotificationEventHandler2);
				}
				while ((object)wlanConnectionNotificationEventHandler != wlanConnectionNotificationEventHandler2);
			}
		}

		public event WlanReasonNotificationEventHandler WlanReasonNotification
		{
			add
			{
				WlanReasonNotificationEventHandler wlanReasonNotificationEventHandler = this.m_0;
				WlanReasonNotificationEventHandler wlanReasonNotificationEventHandler2;
				do
				{
					wlanReasonNotificationEventHandler2 = wlanReasonNotificationEventHandler;
					WlanReasonNotificationEventHandler value2 = (WlanReasonNotificationEventHandler)Delegate.Combine(wlanReasonNotificationEventHandler2, value);
					wlanReasonNotificationEventHandler = Interlocked.CompareExchange(ref this.m_0, value2, wlanReasonNotificationEventHandler2);
				}
				while ((object)wlanReasonNotificationEventHandler != wlanReasonNotificationEventHandler2);
			}
			remove
			{
				WlanReasonNotificationEventHandler wlanReasonNotificationEventHandler = this.m_0;
				WlanReasonNotificationEventHandler wlanReasonNotificationEventHandler2;
				do
				{
					wlanReasonNotificationEventHandler2 = wlanReasonNotificationEventHandler;
					WlanReasonNotificationEventHandler value2 = (WlanReasonNotificationEventHandler)Delegate.Remove(wlanReasonNotificationEventHandler2, value);
					wlanReasonNotificationEventHandler = Interlocked.CompareExchange(ref this.m_0, value2, wlanReasonNotificationEventHandler2);
				}
				while ((object)wlanReasonNotificationEventHandler != wlanReasonNotificationEventHandler2);
			}
		}

		internal WlanInterface(WlanClient P_0, Wlan.WlanInterfaceInfo P_1)
		{
			this.m_0 = P_0;
			this.m_0 = P_1;
		}

		private void _0(Wlan.WlanIntfOpcode P_0, int P_1)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(4);
			Marshal.WriteInt32(intPtr, P_1);
			try
			{
				Wlan._0(Wlan.WlanSetInterface(this.m_0.m_0, this.m_0.interfaceGuid, P_0, 4u, intPtr, IntPtr.Zero));
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		private int _0(Wlan.WlanIntfOpcode P_0)
		{
			Wlan._0(Wlan.WlanQueryInterface(this.m_0.m_0, this.m_0.interfaceGuid, P_0, IntPtr.Zero, out var _, out var ppData, out var _));
			try
			{
				return Marshal.ReadInt32(ppData);
			}
			finally
			{
				Wlan.WlanFreeMemory(ppData);
			}
		}

		public bool SetRadioState(Guid interfaceGuid, Wlan.Dot11RadioState radioState)
		{
			Wlan.WlanPhyRadioState wlanPhyRadioState = new Wlan.WlanPhyRadioState
			{
				dwPhyIndex = 0,
				dot11SoftwareRadioState = radioState
			};
			int num = Marshal.SizeOf(wlanPhyRadioState);
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = Marshal.AllocHGlobal(num);
				Marshal.StructureToPtr(wlanPhyRadioState, intPtr, fDeleteOld: false);
				IntPtr clientHandle = IntPtr.Zero;
				try
				{
					if (Wlan.WlanOpenHandle(2u, IntPtr.Zero, out var _, out clientHandle) != 0)
					{
						return false;
					}
					int num2 = Wlan.WlanSetInterface(clientHandle, interfaceGuid, Wlan.WlanIntfOpcode.RadioState, (uint)num, intPtr, IntPtr.Zero);
					return num2 == 0;
				}
				finally
				{
					Wlan.WlanCloseHandle(clientHandle, IntPtr.Zero);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		public void Scan()
		{
			Wlan._0(Wlan.WlanScan(this.m_0.m_0, this.m_0.interfaceGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
		}

		private static Wlan.WlanAvailableNetwork[] _0(IntPtr P_0)
		{
			Wlan.WlanAvailableNetworkListHeader wlanAvailableNetworkListHeader = (Wlan.WlanAvailableNetworkListHeader)Marshal.PtrToStructure(P_0, typeof(Wlan.WlanAvailableNetworkListHeader));
			long num = P_0.ToInt64() + Marshal.SizeOf(typeof(Wlan.WlanAvailableNetworkListHeader));
			Wlan.WlanAvailableNetwork[] array = new Wlan.WlanAvailableNetwork[wlanAvailableNetworkListHeader.numberOfItems];
			for (int i = 0; i < wlanAvailableNetworkListHeader.numberOfItems; i++)
			{
				ref Wlan.WlanAvailableNetwork reference = ref array[i];
				reference = (Wlan.WlanAvailableNetwork)Marshal.PtrToStructure(new IntPtr(num), typeof(Wlan.WlanAvailableNetwork));
				num += Marshal.SizeOf(typeof(Wlan.WlanAvailableNetwork));
			}
			return array;
		}

		public Wlan.WlanAvailableNetwork[] GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags flags)
		{
			Wlan._0(Wlan.WlanGetAvailableNetworkList(this.m_0.m_0, this.m_0.interfaceGuid, flags, IntPtr.Zero, out var availableNetworkListPtr));
			try
			{
				return _0(availableNetworkListPtr);
			}
			finally
			{
				Wlan.WlanFreeMemory(availableNetworkListPtr);
			}
		}

		private static Wlan.WlanBssEntry[] _0(IntPtr P_0)
		{
			Wlan.WlanBssListHeader wlanBssListHeader = (Wlan.WlanBssListHeader)Marshal.PtrToStructure(P_0, typeof(Wlan.WlanBssListHeader));
			long num = P_0.ToInt64() + Marshal.SizeOf(typeof(Wlan.WlanBssListHeader));
			Wlan.WlanBssEntry[] array = new Wlan.WlanBssEntry[wlanBssListHeader.numberOfItems];
			for (int i = 0; i < wlanBssListHeader.numberOfItems; i++)
			{
				ref Wlan.WlanBssEntry reference = ref array[i];
				reference = (Wlan.WlanBssEntry)Marshal.PtrToStructure(new IntPtr(num), typeof(Wlan.WlanBssEntry));
				num += Marshal.SizeOf(typeof(Wlan.WlanBssEntry));
			}
			return array;
		}

		public Wlan.WlanBssEntry[] GetNetworkBssList()
		{
			Wlan._0(Wlan.WlanGetNetworkBssList(this.m_0.m_0, this.m_0.interfaceGuid, IntPtr.Zero, Wlan.Dot11BssType.Any, securityEnabled: false, IntPtr.Zero, out var wlanBssList));
			try
			{
				return _0(wlanBssList);
			}
			finally
			{
				Wlan.WlanFreeMemory(wlanBssList);
			}
		}

		public Wlan.WlanBssEntry[] GetNetworkBssList(Wlan.Dot11Ssid ssid, Wlan.Dot11BssType bssType, bool securityEnabled)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ssid));
			Marshal.StructureToPtr(ssid, intPtr, fDeleteOld: false);
			try
			{
				Wlan._0(Wlan.WlanGetNetworkBssList(this.m_0.m_0, this.m_0.interfaceGuid, intPtr, bssType, securityEnabled, IntPtr.Zero, out var wlanBssList));
				try
				{
					return _0(wlanBssList);
				}
				finally
				{
					Wlan.WlanFreeMemory(wlanBssList);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		protected void _0(Wlan.WlanConnectionParameters P_0)
		{
			Wlan._0(Wlan.WlanConnect(this.m_0.m_0, this.m_0.interfaceGuid, ref P_0, IntPtr.Zero));
		}

		public void Connect(Wlan.WlanConnectionMode connectionMode, Wlan.Dot11BssType bssType, string profile)
		{
			_0(new Wlan.WlanConnectionParameters
			{
				wlanConnectionMode = connectionMode,
				profile = profile,
				dot11BssType = bssType,
				flags = (Wlan.WlanConnectionFlags)0
			});
		}

		public bool ConnectSynchronously(Wlan.WlanConnectionMode connectionMode, Wlan.Dot11BssType bssType, string profile, int connectTimeout)
		{
			this.m_0 = true;
			try
			{
				Connect(connectionMode, bssType, profile);
				while (this.m_0 && this.m_0.WaitOne(connectTimeout, exitContext: true))
				{
					lock (this.m_0)
					{
						while (this.m_0.Count != 0)
						{
							if (this.m_0.Dequeue() is WlanConnectionNotificationEventData wlanConnectionNotificationEventData)
							{
								if (wlanConnectionNotificationEventData.notifyData.notificationSource == Wlan.WlanNotificationSource.ACM)
								{
									Wlan.WlanNotificationCodeAcm notificationCode = (Wlan.WlanNotificationCodeAcm)wlanConnectionNotificationEventData.notifyData.notificationCode;
									if (notificationCode == Wlan.WlanNotificationCodeAcm.ConnectionComplete && wlanConnectionNotificationEventData.connNotifyData.profileName == profile)
									{
										return true;
									}
									break;
								}
								break;
							}
						}
					}
				}
			}
			finally
			{
				this.m_0 = false;
				this.m_0.Clear();
			}
			return false;
		}

		public void Connect(Wlan.WlanConnectionMode connectionMode, Wlan.Dot11BssType bssType, Wlan.Dot11Ssid ssid, Wlan.WlanConnectionFlags flags)
		{
			Wlan.WlanConnectionParameters wlanConnectionParameters = new Wlan.WlanConnectionParameters
			{
				wlanConnectionMode = connectionMode,
				dot11SsidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ssid))
			};
			Marshal.StructureToPtr(ssid, wlanConnectionParameters.dot11SsidPtr, fDeleteOld: false);
			wlanConnectionParameters.dot11BssType = bssType;
			wlanConnectionParameters.flags = flags;
			_0(wlanConnectionParameters);
			Marshal.DestroyStructure(wlanConnectionParameters.dot11SsidPtr, ssid.GetType());
			Marshal.FreeHGlobal(wlanConnectionParameters.dot11SsidPtr);
		}

		public void Disconnect()
		{
			Wlan._0(Wlan.WlanDisconnect(this.m_0.m_0, this.m_0.interfaceGuid, IntPtr.Zero));
		}

		public void DeleteProfile(string profileName)
		{
			Wlan._0(Wlan.WlanDeleteProfile(this.m_0.m_0, this.m_0.interfaceGuid, profileName, IntPtr.Zero));
		}

		public Wlan.WlanReasonCode SetProfile(Wlan.WlanProfileFlags flags, string profileXml, bool overwrite)
		{
			Wlan._0(Wlan.WlanSetProfile(this.m_0.m_0, this.m_0.interfaceGuid, flags, profileXml, null, overwrite, IntPtr.Zero, out var reasonCode));
			return reasonCode;
		}

		public string GetProfileXml(string profileName)
		{
			Wlan._0(Wlan.WlanGetProfile(this.m_0.m_0, this.m_0.interfaceGuid, profileName, IntPtr.Zero, out var profileXml, out var _, out var _));
			try
			{
				return Marshal.PtrToStringUni(profileXml);
			}
			finally
			{
				Wlan.WlanFreeMemory(profileXml);
			}
		}

		public string GetProfileXmlUnencrypted(string profileName)
		{
			Wlan.WlanProfileFlags flags = Wlan.WlanProfileFlags.GetPlaintextKey;
			Wlan._0(Wlan.WlanGetProfile(this.m_0.m_0, this.m_0.interfaceGuid, profileName, IntPtr.Zero, out var profileXml, out flags, out var _));
			try
			{
				return Marshal.PtrToStringUni(profileXml);
			}
			finally
			{
				Wlan.WlanFreeMemory(profileXml);
			}
		}

		public Wlan.WlanProfileInfo[] GetProfiles()
		{
			Wlan._0(Wlan.WlanGetProfileList(this.m_0.m_0, this.m_0.interfaceGuid, IntPtr.Zero, out var profileList));
			try
			{
				Wlan.WlanProfileInfoListHeader wlanProfileInfoListHeader = (Wlan.WlanProfileInfoListHeader)Marshal.PtrToStructure(profileList, typeof(Wlan.WlanProfileInfoListHeader));
				Wlan.WlanProfileInfo[] array = new Wlan.WlanProfileInfo[wlanProfileInfoListHeader.numberOfItems];
				long num = profileList.ToInt64() + Marshal.SizeOf(wlanProfileInfoListHeader);
				for (int i = 0; i < wlanProfileInfoListHeader.numberOfItems; i++)
				{
					Wlan.WlanProfileInfo wlanProfileInfo = (Wlan.WlanProfileInfo)Marshal.PtrToStructure(new IntPtr(num), typeof(Wlan.WlanProfileInfo));
					array[i] = wlanProfileInfo;
					num += Marshal.SizeOf(wlanProfileInfo);
				}
				return array;
			}
			finally
			{
				Wlan.WlanFreeMemory(profileList);
			}
		}

		internal void _0(Wlan.WlanNotificationData P_0, Wlan.WlanConnectionNotificationData P_1)
		{
			if (this.m_0 != null)
			{
				this.m_0(P_0, P_1);
			}
			if (this.m_0)
			{
				_1(new WlanConnectionNotificationEventData
				{
					notifyData = P_0,
					connNotifyData = P_1
				});
			}
		}

		internal void _0(Wlan.WlanNotificationData P_0, Wlan.WlanReasonCode P_1)
		{
			if (this.m_0 != null)
			{
				this.m_0(P_0, P_1);
			}
			if (this.m_0)
			{
				_1(new WlanReasonNotificationData
				{
					notifyData = P_0,
					reasonCode = P_1
				});
			}
		}

		internal void _1(Wlan.WlanNotificationData P_0)
		{
			if (this.m_0 != null)
			{
				this.m_0(P_0);
			}
		}

		private void _1(object P_0)
		{
			lock (this.m_0)
			{
				this.m_0.Enqueue(P_0);
			}
			this.m_0.Set();
		}
	}

	private IntPtr m_0;

	private uint m_0;

	private readonly Wlan.WlanNotificationCallbackDelegate m_0;

	private readonly Dictionary<Guid, WlanInterface> m_0 = new Dictionary<Guid, WlanInterface>();

	public WlanInterface[] Interfaces
	{
		get
		{
			Wlan._0(Wlan.WlanEnumInterfaces(this.m_0, IntPtr.Zero, out var ppInterfaceList));
			try
			{
				Wlan.WlanInterfaceInfoListHeader wlanInterfaceInfoListHeader = (Wlan.WlanInterfaceInfoListHeader)Marshal.PtrToStructure(ppInterfaceList, typeof(Wlan.WlanInterfaceInfoListHeader));
				long num = ppInterfaceList.ToInt64() + Marshal.SizeOf(wlanInterfaceInfoListHeader);
				WlanInterface[] array = new WlanInterface[wlanInterfaceInfoListHeader.numberOfItems];
				List<Guid> list = new List<Guid>();
				for (int i = 0; i < wlanInterfaceInfoListHeader.numberOfItems; i++)
				{
					Wlan.WlanInterfaceInfo wlanInterfaceInfo = (Wlan.WlanInterfaceInfo)Marshal.PtrToStructure(new IntPtr(num), typeof(Wlan.WlanInterfaceInfo));
					num += Marshal.SizeOf(wlanInterfaceInfo);
					list.Add(wlanInterfaceInfo.interfaceGuid);
					if (!this.m_0.TryGetValue(wlanInterfaceInfo.interfaceGuid, out var value))
					{
						value = new WlanInterface(this, wlanInterfaceInfo);
						this.m_0[wlanInterfaceInfo.interfaceGuid] = value;
					}
					array[i] = value;
				}
				Queue<Guid> queue = new Queue<Guid>();
				foreach (Guid key2 in this.m_0.Keys)
				{
					if (!list.Contains(key2))
					{
						queue.Enqueue(key2);
					}
				}
				while (queue.Count != 0)
				{
					Guid key = queue.Dequeue();
					this.m_0.Remove(key);
				}
				return array;
			}
			finally
			{
				Wlan.WlanFreeMemory(ppInterfaceList);
			}
		}
	}

	public WlanClient()
	{
		Wlan._0(Wlan.WlanOpenHandle(1u, IntPtr.Zero, out this.m_0, out this.m_0));
		try
		{
			this.m_0 = _0;
			Wlan._0(Wlan.WlanRegisterNotification(this.m_0, Wlan.WlanNotificationSource.All, ignoreDuplicate: false, this.m_0, IntPtr.Zero, IntPtr.Zero, out var _));
		}
		catch
		{
			_8();
			throw;
		}
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
		_8();
	}

	~WlanClient()
	{
		_8();
	}

	private void _8()
	{
		if (this.m_0 != IntPtr.Zero)
		{
			Wlan.WlanCloseHandle(this.m_0, IntPtr.Zero);
			this.m_0 = IntPtr.Zero;
		}
	}

	private static Wlan.WlanConnectionNotificationData? _0(ref Wlan.WlanNotificationData P_0)
	{
		int num = Marshal.SizeOf(typeof(Wlan.WlanConnectionNotificationData));
		if (P_0.dataSize < num)
		{
			return null;
		}
		Wlan.WlanConnectionNotificationData value = (Wlan.WlanConnectionNotificationData)Marshal.PtrToStructure(P_0.dataPtr, typeof(Wlan.WlanConnectionNotificationData));
		if (value.wlanReasonCode == Wlan.WlanReasonCode.Success)
		{
			IntPtr ptr = new IntPtr(P_0.dataPtr.ToInt64() + Marshal.OffsetOf(typeof(Wlan.WlanConnectionNotificationData), "profileXml").ToInt64());
			value.profileXml = Marshal.PtrToStringUni(ptr);
		}
		return value;
	}

	private void _0(ref Wlan.WlanNotificationData P_0, IntPtr P_1)
	{
		this.m_0.TryGetValue(P_0.interfaceGuid, out var value);
		switch (P_0.notificationSource)
		{
		case Wlan.WlanNotificationSource.ACM:
			switch ((Wlan.WlanNotificationCodeAcm)P_0.notificationCode)
			{
			case Wlan.WlanNotificationCodeAcm.ConnectionStart:
			case Wlan.WlanNotificationCodeAcm.ConnectionComplete:
			case Wlan.WlanNotificationCodeAcm.ConnectionAttemptFail:
			case Wlan.WlanNotificationCodeAcm.Disconnecting:
			case Wlan.WlanNotificationCodeAcm.Disconnected:
			{
				Wlan.WlanConnectionNotificationData? wlanConnectionNotificationData2 = _0(ref P_0);
				if (wlanConnectionNotificationData2.HasValue)
				{
					value?._0(P_0, wlanConnectionNotificationData2.Value);
				}
				break;
			}
			case Wlan.WlanNotificationCodeAcm.ScanFail:
			{
				int num = Marshal.SizeOf(typeof(int));
				if (P_0.dataSize >= num)
				{
					Wlan.WlanReasonCode wlanReasonCode = (Wlan.WlanReasonCode)Marshal.ReadInt32(P_0.dataPtr);
					value?._0(P_0, wlanReasonCode);
				}
				break;
			}
			}
			break;
		case Wlan.WlanNotificationSource.MSM:
			switch ((Wlan.WlanNotificationCodeMsm)P_0.notificationCode)
			{
			case Wlan.WlanNotificationCodeMsm.Associating:
			case Wlan.WlanNotificationCodeMsm.Associated:
			case Wlan.WlanNotificationCodeMsm.Authenticating:
			case Wlan.WlanNotificationCodeMsm.Connected:
			case Wlan.WlanNotificationCodeMsm.RoamingStart:
			case Wlan.WlanNotificationCodeMsm.RoamingEnd:
			case Wlan.WlanNotificationCodeMsm.Disassociating:
			case Wlan.WlanNotificationCodeMsm.Disconnected:
			case Wlan.WlanNotificationCodeMsm.PeerJoin:
			case Wlan.WlanNotificationCodeMsm.PeerLeave:
			case Wlan.WlanNotificationCodeMsm.AdapterRemoval:
			{
				Wlan.WlanConnectionNotificationData? wlanConnectionNotificationData = _0(ref P_0);
				if (wlanConnectionNotificationData.HasValue)
				{
					value?._0(P_0, wlanConnectionNotificationData.Value);
				}
				break;
			}
			}
			break;
		}
		value?._1(P_0);
	}

	public string GetStringForReasonCode(Wlan.WlanReasonCode reasonCode)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		Wlan._0(Wlan.WlanReasonCodeToString(reasonCode, stringBuilder.Capacity, stringBuilder, IntPtr.Zero));
		return stringBuilder.ToString();
	}
}
