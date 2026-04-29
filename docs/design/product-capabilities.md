# Product Design — Capabilities over Types

## Overview

This ERP intentionally avoids using a rigid `ItemType` or `ProductType` enum such as:

- Product
- Service
- Supply
- Fixed Asset

Instead, products are modeled using **capabilities** that describe how they behave in the system.

This keeps the inventory, purchasing, and sales domains clean, flexible, and decoupled from accounting and pricing rules.

---

## Why not `ItemType`?

In traditional ERPs (Softland, SAP, etc.), a product is often classified as:
- Product
- Service
- Asset
- Supply

This creates multiple problems:

- It mixes **operational reality** (stock, warehouses) with **accounting concepts**
- It forces conditional logic everywhere (`if type == Service ...`)
- It prevents a product from changing its role depending on context
- It makes multi-company SaaS harder to evolve

In the real world:
- The same item can be sold, consumed internally, or capitalized as an asset
- What matters is **how the item behaves**, not what it is called

---

## Design Principle

> **Inventory models physical reality.  
Business and accounting models describe how that reality is used.**

Therefore:

- Inventory must not depend on accounting or pricing abstractions
- Product classification must be flexible and context-driven

---

## Product Capabilities

Instead of a `Type`, each product has **capabilities**:

| Capability      | Meaning |
|-----------------|--------|
| `IsStockable`   | The product generates stock and inventory movements |
| `IsSellable`    | The product can appear on sales documents |
| `IsPurchasable` | The product can be bought from suppliers |

These flags allow modeling all real-world cases:

| Real-world item | IsStockable | IsSellable | IsPurchasable |
|----------------|------------|------------|----------------|
| Physical product | true | true | true |
| Service | false | true | false |
| Supply / raw material | true | false | true |
| Non-stock purchase (e.g. consulting) | false | false | true |

No enum is required.

---

## Domain Rules

The Product domain enforces only one invariant:

> **If a product is stockable, it must be either sellable or purchasable.**

A stockable product that is neither bought nor sold is meaningless.

This keeps inventory consistent without introducing accounting logic into the product model.

---

## What Product does NOT contain

The Product entity deliberately does not include:

- VAT / Tax rates  
- Inventory accounts  
- Cost of goods sold accounts  
- Revenue accounts  
- Fixed asset classification  
- Serial or lot tracking  

These belong to other bounded contexts:

| Concern | Module |
|--------|--------|
| Taxes | Tax |
| Accounting rules | Accounting |
| Fixed assets | FixedAssets |
| Pricing | Sales / Billing |
| Serial / lot tracking | Inventory (optional future extension) |

This keeps Master Data clean and reusable.

---

## Benefits

This design:
- Avoids legacy ERP coupling
- Allows services, goods, and supplies to coexist naturally
- Keeps inventory fast and simple
- Allows accounting, tax, and asset rules to evolve independently
- Scales cleanly to multi-tenant SaaS

---

## Summary

> **Products are defined by what they can do, not by what they are called.**

Capabilities over types is a core design decision of this ERP.
