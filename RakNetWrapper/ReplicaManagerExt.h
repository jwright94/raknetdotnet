#ifndef __ReplicaManagerExt_H_
#define __ReplicaManagerExt_H_

#include "ReplicaManager.h"

class ReplicaManagerWithBackdoor : public ReplicaManager
{
public:
	// Plugin interface functions
	void Update(RakPeerInterface *peer) { ReplicaManager::Update(peer); }
	void OnAttach(RakPeerInterface *peer) { ReplicaManager::OnAttach(peer); }
	PluginReceiveResult OnReceive(RakPeerInterface *peer, Packet *packet) { return ReplicaManager::OnReceive(peer, packet); }
	void OnCloseConnection(RakPeerInterface *peer, SystemAddress systemAddress) { ReplicaManager::OnCloseConnection(peer, systemAddress); }
	void OnShutdown(RakPeerInterface *peer) { ReplicaManager::OnShutdown(peer); }
};

class ReplicaManagerExt;

typedef ReplicaReturnResult (* RME_FP_ConstructionCB)(RakNet::BitStream *inBitStream, RakNetTime timestamp, NetworkID *new_networkID, Replica *existingReplica, SystemAddress *new_senderId, ReplicaManagerExt *caller, void *userData);
typedef ReplicaReturnResult (* RME_FP_SendDownloadCompleteCB)(RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress *new_senderId, ReplicaManagerExt *caller, void *userData);
typedef ReplicaReturnResult (* RME_FP_ReceiveDownloadCompleteCB)(RakNet::BitStream *inBitStream, SystemAddress* new_senderId, ReplicaManagerExt *caller, void *userData);

class ReplicaManagerExt : public PluginInterface
{
public:
	ReplicaManagerExt() 
	{
		myOBJ.SetReceiveConstructionCB(this, CallConstructionCB);
		myOBJ.SetDownloadCompleteCB(this, CallSendDownloadCompleteCB, this, CallReceiveDownloadCompleteCB);
	}

	// extend
	void SetReceiveConstructionCB(void *_constructionUserData, RME_FP_ConstructionCB constructionCB) 
	{
		// Just overwrite the construction callback pointer
		_constructionCB=constructionCB;
		constructionUserData=_constructionUserData;	
	}
	void SetDownloadCompleteCB(void *_sendDownloadCompleteUserData, RME_FP_SendDownloadCompleteCB sendDownloadCompleteCB, 
		void *_receiveDownloadCompleteUserData, RME_FP_ReceiveDownloadCompleteCB receiveDownloadCompleteCB) 
	{
		// Just overwrite the send and receive download complete pointers.
		_sendDownloadCompleteCB=sendDownloadCompleteCB;
		_receiveDownloadCompleteCB=receiveDownloadCompleteCB;

		sendDownloadCompleteUserData=_sendDownloadCompleteUserData;
		receiveDownloadCompleteUserData=_receiveDownloadCompleteUserData;
	}

	// Original interfaces
	void SetAutoParticipateNewConnections(bool autoAdd) { myOBJ.SetAutoParticipateNewConnections(autoAdd); }
	bool AddParticipant(SystemAddress systemAddress) { return myOBJ.AddParticipant(systemAddress); }
	bool RemoveParticipant(SystemAddress systemAddress) { return myOBJ.RemoveParticipant(systemAddress); }
	void Construct(Replica *replica, bool isCopy, SystemAddress systemAddress, bool broadcast) { myOBJ.Construct(replica, isCopy, systemAddress, broadcast); }
	void Destruct(Replica *replica, SystemAddress systemAddress, bool broadcast) { myOBJ.Destruct(replica, systemAddress, broadcast); }
	void ReferencePointer(Replica *replica) { myOBJ.ReferencePointer(replica); }
	void DereferencePointer(Replica *replica) { myOBJ.DereferencePointer(replica); }
	void SetScope(Replica *replica, bool inScope, SystemAddress systemAddress, bool broadcast) { myOBJ.SetScope(replica, inScope, systemAddress, broadcast); }
	void SignalSerializeNeeded(Replica *replica, SystemAddress systemAddress, bool broadcast) { myOBJ.SignalSerializeNeeded(replica, systemAddress, broadcast); }
	// hooks these methods
	//void SetReceiveConstructionCB(void *_constructionUserData, ReplicaReturnResult (* constructionCB)(RakNet::BitStream *inBitStream, RakNetTime timestamp, NetworkID networkID, Replica *existingReplica, SystemAddress senderId, ReplicaManager *caller, void *userData)) { myOBJ.SetReceiveConstructionCB(_constructionUserData, constructionCB); }
	//void SetDownloadCompleteCB(void *_sendDownloadCompleteUserData, ReplicaReturnResult (* sendDownloadCompleteCB)(RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress senderId, ReplicaManager *caller, void *userData),
	//	void *_receiveDownloadCompleteUserData, ReplicaReturnResult (* receiveDownloadCompleteCB)(RakNet::BitStream *inBitStream, SystemAddress senderId, ReplicaManager *caller, void *userData)) { myOBJ.SetDownloadCompleteCB(_sendDownloadCompleteUserData, sendDownloadCompleteCB, _receiveDownloadCompleteUserData, receiveDownloadCompleteCB); }
	void SetSendChannel(unsigned char channel) { myOBJ.SetSendChannel(channel); }
	void SetAutoConstructToNewParticipants(bool autoConstruct) { myOBJ.SetAutoConstructToNewParticipants(autoConstruct); }
	void SetDefaultScope(bool scope) { myOBJ.SetDefaultScope(scope); }
	void SetAutoSerializeInScope(bool autoSerialize) { myOBJ.SetAutoSerializeInScope(autoSerialize); }
	void EnableReplicaInterfaces(Replica *replica, unsigned char interfaceFlags) { myOBJ.EnableReplicaInterfaces(replica, interfaceFlags); }
	void DisableReplicaInterfaces(Replica *replica, unsigned char interfaceFlags) { myOBJ.DisableReplicaInterfaces(replica, interfaceFlags); } 
	bool IsConstructed(Replica *replica, SystemAddress systemAddress) { return myOBJ.IsConstructed(replica, systemAddress); }
	bool IsInScope(Replica *replica, SystemAddress systemAddress) { return myOBJ.IsInScope(replica, systemAddress); }
	unsigned GetReplicaCount(void) const { return myOBJ.GetReplicaCount(); }
	Replica *GetReplicaAtIndex(unsigned index) { return myOBJ.GetReplicaAtIndex(index); }
	unsigned GetParticipantCount(void) const { return myOBJ.GetParticipantCount(); }
	SystemAddress GetParticipantAtIndex(unsigned index) { return myOBJ.GetParticipantAtIndex(index); }
	void SignalSerializationFlags(Replica *replica, SystemAddress systemAddress, bool broadcast, bool set, unsigned int flags) { myOBJ.SignalSerializationFlags(replica, systemAddress, broadcast, set, flags); }
	unsigned int* AccessSerializationFlags(Replica *replica, SystemAddress systemAddress) { return myOBJ.AccessSerializationFlags(replica, systemAddress); }

protected:
	// Plugin interface functions
	void Update(RakPeerInterface *peer) { myOBJ.Update(peer); }
	void OnAttach(RakPeerInterface *peer) { myOBJ.OnAttach(peer); }
	PluginReceiveResult OnReceive(RakPeerInterface *peer, Packet *packet) { return myOBJ.OnReceive(peer, packet); }
	void OnCloseConnection(RakPeerInterface *peer, SystemAddress systemAddress) { myOBJ.OnCloseConnection(peer, systemAddress); }
	void OnShutdown(RakPeerInterface *peer) { myOBJ.OnShutdown(peer); }

	static ReplicaReturnResult CallConstructionCB(RakNet::BitStream *inBitStream, RakNetTime timestamp, NetworkID networkID, Replica *existingReplica, SystemAddress senderId, ReplicaManager *caller, void *userData)
	{
		ReplicaManagerExt* ext = reinterpret_cast<ReplicaManagerExt*>(userData);
		return ext->_constructionCB(inBitStream, timestamp, new NetworkID(networkID), existingReplica, new SystemAddress(senderId), ext, ext->constructionUserData);
	}
	static ReplicaReturnResult CallSendDownloadCompleteCB(RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress senderId, ReplicaManager *caller, void *userData)
	{
		ReplicaManagerExt* ext = reinterpret_cast<ReplicaManagerExt*>(userData);
		return ext->_sendDownloadCompleteCB(outBitStream, currentTime, new SystemAddress(senderId), ext, ext->sendDownloadCompleteUserData);
	}
	static ReplicaReturnResult CallReceiveDownloadCompleteCB(RakNet::BitStream *inBitStream, SystemAddress senderId, ReplicaManager *caller, void *userData)
	{
		ReplicaManagerExt* ext = reinterpret_cast<ReplicaManagerExt*>(userData);
		return ext->_receiveDownloadCompleteCB(inBitStream, new SystemAddress(senderId), ext, ext->receiveDownloadCompleteUserData);
	}

	// Required callback to handle construction calls
	RME_FP_ConstructionCB _constructionCB;

	// Optional callbacks to send and receive download complete.
	RME_FP_SendDownloadCompleteCB _sendDownloadCompleteCB;
	RME_FP_ReceiveDownloadCompleteCB _receiveDownloadCompleteCB;

	// Userdata with the callbacks
	void *receiveDownloadCompleteUserData, *sendDownloadCompleteUserData, *constructionUserData;

	ReplicaManagerWithBackdoor myOBJ;
};

#endif //__ReplicaManagerExt_H_