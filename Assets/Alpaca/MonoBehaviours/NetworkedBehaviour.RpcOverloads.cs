using Alpaca.Data;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Alpaca
{
	/*
	 * Welcome, hope you made it here without your IDE crashing (*cough* Rider)
	 * This file is automatically genererated and contains all the RPC overloads.
	 * Conveience methods include up to 32 parameters.
	 * The generation script can be found here (Heads up, it's not pretty)
	 * https://gist.github.com/TwoTenPvP/6dd0fbfa8ec34329c0e219281779c935
	 */
    public abstract partial class Conduct : MonoBehaviour
    {
		/*
		#region SEND METHODS
		/// <exclude />
		public delegate void RpcMethod();
		/// <exclude />
		public delegate void RpcMethod<T1>(T1 t1);
		/// <exclude />
		public delegate void RpcMethod<T1, T2>(T1 t1, T2 t2);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11);
		/// <exclude />
		public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult>();
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1>(T1 t1);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2>(T1 t1, T2 t2);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3>(T1 t1, T2 t2, T3 t3);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11);
		/// <exclude />
		public delegate TResult ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12);


		#region BOXED CLIENT RPC

		public void InvokeClientRpcOnOwner(RpcMethod method, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security);
		}

		public void InvokeClientRpcOnOwner(string methodName, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security);
		}

		public void InvokeClientRpcOnEveryone(RpcMethod method, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security);
		}

		public void InvokeClientRpcOnEveryone(string methodName, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security);
		}

		public void InvokeClientRpcOnClient(RpcMethod method, uint clientId, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security);
		}

		public void InvokeClientRpcOnClient(string methodName, uint clientId, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security);
		}

		public void InvokeClientRpcOnEveryoneExcept(RpcMethod method, uint clientIdToIgnore, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security);
		}

		public void InvokeClientRpcOnEveryoneExcept(string methodName, uint clientIdToIgnore, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security);
		}

		public void InvokeClientRpcOnOwner<T1>(RpcMethod<T1> method, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1>(ResponseRpcMethod<TResult, T1> method, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1);
		}

		public void InvokeClientRpcOnOwner<T1>(string methodName, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1>(string methodName, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1);
		}

		public void InvokeClientRpcOnEveryone<T1>(RpcMethod<T1> method, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1);
		}

		public void InvokeClientRpcOnEveryone<T1>(string methodName, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1);
		}

		public void InvokeClientRpcOnClient<T1>(RpcMethod<T1> method, uint clientId, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1>(ResponseRpcMethod<TResult, T1> method, uint clientId, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1);
		}

		public void InvokeClientRpcOnClient<T1>(string methodName, uint clientId, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1>(string methodName, uint clientId, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1>(RpcMethod<T1> method, uint clientIdToIgnore, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1>(string methodName, uint clientIdToIgnore, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1);
		}

		public void InvokeClientRpcOnOwner<T1, T2>(RpcMethod<T1, T2> method, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2>(ResponseRpcMethod<TResult, T1, T2> method, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2);
		}

		public void InvokeClientRpcOnOwner<T1, T2>(string methodName, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2>(string methodName, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2);
		}

		public void InvokeClientRpcOnEveryone<T1, T2>(RpcMethod<T1, T2> method, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2);
		}

		public void InvokeClientRpcOnEveryone<T1, T2>(string methodName, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2);
		}

		public void InvokeClientRpcOnClient<T1, T2>(RpcMethod<T1, T2> method, uint clientId, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2>(ResponseRpcMethod<TResult, T1, T2> method, uint clientId, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2);
		}

		public void InvokeClientRpcOnClient<T1, T2>(string methodName, uint clientId, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2>(string methodName, uint clientId, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2>(RpcMethod<T1, T2> method, uint clientIdToIgnore, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3>(RpcMethod<T1, T2, T3> method, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3>(ResponseRpcMethod<TResult, T1, T2, T3> method, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3>(string methodName, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3>(string methodName, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3>(RpcMethod<T1, T2, T3> method, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3>(string methodName, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3>(RpcMethod<T1, T2, T3> method, uint clientId, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3>(ResponseRpcMethod<TResult, T1, T2, T3> method, uint clientId, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3>(RpcMethod<T1, T2, T3> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4>(RpcMethod<T1, T2, T3, T4> method, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4>(ResponseRpcMethod<TResult, T1, T2, T3, T4> method, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4>(RpcMethod<T1, T2, T3, T4> method, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4>(RpcMethod<T1, T2, T3, T4> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4>(ResponseRpcMethod<TResult, T1, T2, T3, T4> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4>(RpcMethod<T1, T2, T3, T4> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5>(RpcMethod<T1, T2, T3, T4, T5> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5>(RpcMethod<T1, T2, T3, T4, T5> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5>(RpcMethod<T1, T2, T3, T4, T5> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5>(RpcMethod<T1, T2, T3, T4, T5> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6>(RpcMethod<T1, T2, T3, T4, T5, T6> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6>(RpcMethod<T1, T2, T3, T4, T5, T6> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6>(RpcMethod<T1, T2, T3, T4, T5, T6> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6>(RpcMethod<T1, T2, T3, T4, T5, T6> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7>(RpcMethod<T1, T2, T3, T4, T5, T6, T7> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7>(RpcMethod<T1, T2, T3, T4, T5, T6, T7> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7>(RpcMethod<T1, T2, T3, T4, T5, T6, T7> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7>(RpcMethod<T1, T2, T3, T4, T5, T6, T7> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnOwner<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public RpcResponse<TResult> InvokeClientRpcOnOwner<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), _ownerClientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnEveryone<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnClient<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public RpcResponse<TResult> InvokeClientRpcOnClient<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, uint clientId, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendClientRPCBoxedResponse<TResult>(HashMethodName(methodName), clientId, channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeClientRpcOnEveryoneExcept<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, uint clientIdToIgnore, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCBoxed(clientIdToIgnore, HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		#endregion

		#region BOXED SERVER RPC
		public void InvokeServerRpc(RpcMethod method, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security);
		}

		public void InvokeServerRpc(string methodName, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult>(ResponseRpcMethod<TResult> method, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult>(string methodName, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security);
		}

		public void InvokeServerRpc<T1>(RpcMethod<T1> method, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1>(ResponseRpcMethod<TResult, T1> method, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1);
		}

		public void InvokeServerRpc<T1>(string methodName, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1>(string methodName, T1 t1, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1);
		}

		public void InvokeServerRpc<T1, T2>(RpcMethod<T1, T2> method, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2>(ResponseRpcMethod<TResult, T1, T2> method, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2);
		}

		public void InvokeServerRpc<T1, T2>(string methodName, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2>(string methodName, T1 t1, T2 t2, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2);
		}

		public void InvokeServerRpc<T1, T2, T3>(RpcMethod<T1, T2, T3> method, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3>(ResponseRpcMethod<TResult, T1, T2, T3> method, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3);
		}

		public void InvokeServerRpc<T1, T2, T3>(string methodName, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3>(string methodName, T1 t1, T2 t2, T3 t3, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3);
		}

		public void InvokeServerRpc<T1, T2, T3, T4>(RpcMethod<T1, T2, T3, T4> method, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4>(ResponseRpcMethod<TResult, T1, T2, T3, T4> method, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4);
		}

		public void InvokeServerRpc<T1, T2, T3, T4>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5>(RpcMethod<T1, T2, T3, T4, T5> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6>(RpcMethod<T1, T2, T3, T4, T5, T6> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7>(RpcMethod<T1, T2, T3, T4, T5, T6, T7> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ResponseRpcMethod<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(method.Method.Name), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public void InvokeServerRpc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCBoxed(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		public RpcResponse<TResult> InvokeServerRpc<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string methodName, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			return SendServerRPCBoxedResponse<TResult>(HashMethodName(methodName), channel, security, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
		}

		#endregion
		#region PERFORMANCE CLIENT RPC
		public void InvokeClientRpcOnOwner(RpcDelegate method, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(method.Method.Name), _ownerClientId, stream, channel, security);
		}
		public void InvokeClientRpcOnClient(RpcDelegate method, uint clientId, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(method.Method.Name), clientId, stream, channel, security);
		}
		public void InvokeClientRpcOnEveryone(RpcDelegate method, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(method.Method.Name), stream, channel, security);
		}
		public void InvokeClientRpcOnEveryoneExcept(RpcDelegate method, uint clientIdToIgnore, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(method.Method.Name), stream, clientIdToIgnore, channel, security);
		}
		public void InvokeClientRpcOnClient(string methodName, uint clientId, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(methodName), clientId, stream, channel, security);
		}
		public void InvokeClientRpcOnOwner(string methodName, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(methodName), _ownerClientId, stream, channel, security);
		}
		public void InvokeClientRpcOnEveryone(string methodName, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(methodName), stream, channel, security);
		}
		public void InvokeClientRpcOnEveryoneExcept(string methodName, uint clientIdToIgnore, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendClientRPCPerformance(HashMethodName(methodName), stream, clientIdToIgnore, channel, security);
		}
		#endregion
		#region PERFORMANCE SERVER RPC
		public void InvokeServerRpc(RpcDelegate method, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCPerformance(HashMethodName(method.Method.Name), stream, channel, security);
		}
		public void InvokeServerRpc(string methodName, Stream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
		{
			SendServerRPCPerformance(HashMethodName(methodName), stream, channel, security);
		}
		#endregion
		#endregion

		*/
    }
}