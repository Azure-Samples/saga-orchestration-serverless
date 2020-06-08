# State

## Validator

```json
[
    {
        "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
        "state": "valid" //valid, invalid, failed, canceled
    }
]
```

## Transfer

```json
[
    {
        "id": "9d58bf8b-e255-4569-b356-2aa68937c062",
        "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
        "operationDate": "12/10/2015 6:18 AM",
        "accountId": "38495",
        "amount": -100.00,
        "description": "transfer to 12309"
    },
    {
        "id": "2532f88b-ca17-4430-a700-36aa1fe64692",
        "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
        "operationDate": "12/10/2015 6:18 AM",
        "accountId": "12309",
        "amount": 100.00,
        "description": "transfer from 38495"
    }
]
```

## Receipt

```json
[
    {
        "id": "2de835db-8286-459f-a1bc-809b551a12a6",
        "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
        "accountFromId": "38495",
        "accountToId": "12309",
        "amount": 100.00,
        "receiptSignature": "8951f6ab43086a04ca04b2ea6978a110f59335c6d48462640afdad0ad201b41b",
        "state": "issued" //issued, failed, canceled
    }
]
```

## Orchestrator

```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00,
    "state": "success", //pendingValidation, errorValidation, pendingTransfer, errorTransfer, pendingReceipt, errorReceipt, success
    "error": {
        "message": "",
        "service": "",
    },
    "receipt": {
        "id": "2de835db-8286-459f-a1bc-809b551a12a6",
        "signature": "8951f6ab43086a04ca04b2ea6978a110f59335c6d48462640afdad0ad201b41b"
    }
}
```

## Event Processor

```json
[
    {
        "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
        "messageId": "ada36984-1c3e-4f75-9bd0-98424a460bff",
        "messageType": "TransferSucceededEvent",
        "source": "Transfer",
        "creationDate": "12/10/2015 6:18 AM"
    }
]
```