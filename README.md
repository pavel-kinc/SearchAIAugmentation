# PromptEnhancer - For Prompt Augmentation
The main goal of the project is a library that provides a flexible, highly configurable **RAG-based pipeline** that systematically uses Knowledge Bases to find relevant context, augment the user's base prompt with that information, and prepare the augmented prompt for reliable output generation by a Large Language Model (LLM). It also provides utilities for utilizing LLMs and other AI services and using the **Semantic Kernel**.

It includes **two demonstration applications** built on the library to showcase various use cases.

This repository contains several projects: 
| Project Name | Role & Description |
| :--- | :--- |
| **[`PromptEnhancer`](#the-library---promptenhancer)** | The **library PromptEnhancer** containing all core logic, generic interfaces, kernel management, and extension methods for prompt augmentation. |
| **[`ConfigurableGoogleSearchDemo`](#the-google-demo)** | Configurable **Google Demo** showcasing usage of the library by leveraging Google Search Engine for relevant context and subsequent augmentation. |
| **[`TaskChatDemo`](#the-task-chat-demo)** | **Chat demo** focusing on using multiple AI providers, getting context from multiple Knowledge Bases working with development tasks. Uses other project **TaskDemoAPI** |
| **`AppHost`** | The **.NET Aspire Orchestrator** responsible for defining, configuring, and launching all application projects in the solution.|
**`ServiceDefaults`** | The **Shared Configuration Project** that centralizes common service configurations, such as logging, tracing, and health checks, used by all demo apps.
| **`PromptEnhancer.Tests`** | The **Test** project, ensuring the core library's functionality and interactions with services are robust and correct. |

## Technology

This entire solution is built using the **.NET 9** platform, leveraging the latest advancements in performance and stability from the .NET ecosystem. 

## Cooperation

The development of the solution is the subject of a **Master Thesis**, conducted in **close cooperation with company BiQ pux a.s.**

## Safe Usage Disclaimer

Please be aware of the functionality within the solution (as it is highly configurable):
* It remains the user's sole responsibility to ensure all final outputs and operational practices are used responsibly, ethically, and strictly within legal boundaries.
* The **`ConfigurableGoogleSearchDemo`** project is designed to integrate external web search or scraping tools to augment prompts with real-time data.
* **By default, this external data feature is deactivated and must be explicitly configured and enabled by the user.**

**Legal Compliance and Terms of Service:** Users are solely responsible for ensuring that any enabled external integration complies fully with the **Terms of Service, Acceptable Use Policies (AUPs), and all applicable laws and regulations** of the respective external service providers (e.g., Google, LLM APIs, etc.). We strongly advise users to exercise caution, respect service terms and conditions, and adhere to rate limits when activating any external search or scraping functionality. Use of such tools is done at your own risk.

### The Library - PromptEnhancer
<a href="[Your External URL]" target="_blank">[The Link Text You Want to Display]</a>
## Overview

```csharp
public class ProductService
{
    private readonly IRepository<Product> _productRepository;

    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        // Library methods handle query construction and execution.
        var products = await _productRepository
            .FindAll(p => p.IsAvailable && p.Stock > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
            
        return products;
    }
}
```
wqewqeqeqwwqeqwewqeqe

### The Google Demo

### The Task Chat Demo