# Commands and Events Contracts

Here you can find the contracts that every service expects to handle.

There are 2 categories of Commands: Regular and Compensation.

* Regular: A command that requests some response from a service.
* Compensation: A command that signals to a service to rollback a previous change. 

## Validator

| Command | Category | Success Events | Failure Events |
|---------|----------|----------------|----------------|
| [ValidateTransferCommand](#ValidateTransferCommand) | Regular | [TransferValidatedEvent](#TransferValidatedEvent) |  [InvalidAmountEvent](#InvalidAmountEvent), [InvalidAccountEvent](#InvalidAccountEvent), [OtherReasonValidationFailedEvent](#OtherReasonValidationFailedEvent) |
| [CancelTransferCommand](#CancelTransferCommand) | Compensation | [TransferCanceledEvent](#TransferCanceledEvent)  | [TransferNotCanceledEvent](#TransferNotCanceledEvent) |       

### Commands
#### ValidateTransferCommand
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00
}
```

#### CancelTransferCommand
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00
}
```

### Events
#### TransferValidatedEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2"
}
```

#### InvalidAmountEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "The amount can not be negative."
    }
}
```

#### InvalidAccountEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "The from account was not found on the system."
    }
}
```

#### OtherReasonValidationFailedEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "A generic error has happened. Please try again."
    }
}
```

#### TransferCanceledEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2"
}
```

#### TransferNotCanceledEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "A generic error has happened. Please try again."
    }
}
```

## Transfer

| Command | Category | Success Events | Failure Events |
|---------|----------|----------------|----------------|
| [TransferCommand](#TransferCommand) | Regular | [TransferSucceededEvent](#TransferSucceededEvent) | [OtherReasonTransferFailedEvent](#OtherReasonTransferFailedEvent) |
| [CancelTransferCommand](#CancelTransferCommand) | Compensation | [TransferCanceledEvent](#TransferCanceledEvent) | [TransferNotCanceledEvent](#TransferNotCanceledEvent) |

### Commands

#### TransferCommand
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00
}
```

#### CancelTransferCommand
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00
}
```

### Events
#### TransferSucceededEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2"
}
```

#### OtherReasonTransferFailedEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "A generic error has happened. Please try again."
    }
}
```

#### TransferCanceledEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2"
}
```

#### TransferNotCanceledEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "A generic error has happened. Please try again."
    }
}
```

## Receipt

| Command | Category | Success Events | Failure Events |
|---------|----------|----------------|----------------|
| [IssueReceiptCommand](#IssueReceiptCommand) | Regular | [ReceiptIssuedEvent](#ReceiptIssuedEvent) | [OtherReasonReceiptFailedEvent](#OtherReasonReceiptFailedEvent) |
| [CancelTransferCommand](#CancelTransferCommand) | Compensation | [TransferCanceledEvent](#TransferCanceledEvent) | [TransferNotCanceledEvent](#TransferNotCanceledEvent) |

### Commands

#### IssueReceiptCommand
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00
}
```

#### CancelTransferCommand
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "accountFromId": "38495",
    "accountToId": "12309",
    "amount": 100.00
}
```

### Events

#### ReceiptIssuedEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "receiptId": "2de835db-8286-459f-a1bc-809b551a12a6",
    "receiptSignature": "8951f6ab43086a04ca04b2ea6978a110f59335c6d48462640afdad0ad201b41b"
}
```

#### OtherReasonReceiptFailedEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "A generic error has happened. Please try again."
    }
}
```

#### TransferCanceledEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2"
}
```

#### TransferNotCanceledEvent
```json
{
    "transactionId": "afc63a8b-5ba6-46cb-97fe-4e0d7740cac2",
    "error": {
        "message": "A generic error has happened. Please try again."
    }
}
```