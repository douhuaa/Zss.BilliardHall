# Blazor Server + WebAssembly 开发模式 (Blazor Development Patterns)

## 总体架构 (Overall Architecture)

本项目采用 Blazor Server + WebAssembly 混合模式：
- **Blazor Server**: 用于管理后台和复杂交互
- **Blazor WebAssembly**: 用于公共页面和客户端富交互
- **Blazorise**: 基于 Bootstrap 5 的 UI 组件库
- **ABP Blazor**: 集成 ABP 框架的 Blazor 支持

## Blazor 组件开发模式

### 1. 页面组件结构

```razor
@* BilliardTableList.razor - 列表页面 *@
@page "/billiard-tables"
@using Zss.BilliardHall.BilliardTables
@using Zss.BilliardHall.Permissions
@inherits BilliardHallComponentBase
@inject IBilliardTableAppService BilliardTableAppService

<Card>
    <CardHeader>
        <Row Class="justify-content-between">
            <Column ColumnSize="ColumnSize.IsAuto">
                <h1>@L["BilliardTables"]</h1>
            </Column>
            <Column ColumnSize="ColumnSize.IsAuto">
                @if (HasCreatePermission)
                {
                    <Button Color="Color.Primary" Clicked="OpenCreateModalAsync">
                        <Icon Name="IconName.Add" class="me-1" />
                        @L["NewBilliardTable"]
                    </Button>
                }
            </Column>
        </Row>
    </CardHeader>
    <CardBody>
        @* 搜索过滤器 *@
        <Form>
            <Row>
                <Column ColumnSize="ColumnSize.Is3">
                    <Field>
                        <FieldLabel>@L["Filter"]</FieldLabel>
                        <TextEdit @bind-Text="Filter.Filter" Placeholder="@L["Search"]" />
                    </Field>
                </Column>
                <Column ColumnSize="ColumnSize.Is2">
                    <Field>
                        <FieldLabel>@L["Type"]</FieldLabel>
                        <Select TValue="BilliardTableType?" @bind-SelectedValue="Filter.Type">
                            <SelectItem TValue="BilliardTableType?" Value="null">@L["All"]</SelectItem>
                            @foreach (var type in Enum.GetValues<BilliardTableType>())
                            {
                                <SelectItem TValue="BilliardTableType?" Value="type">
                                    @L[$"Enum:BilliardTableType.{type}"]
                                </SelectItem>
                            }
                        </Select>
                    </Field>
                </Column>
                <Column ColumnSize="ColumnSize.Is2">
                    <Field>
                        <FieldLabel>@L["Status"]</FieldLabel>
                        <Select TValue="BilliardTableStatus?" @bind-SelectedValue="Filter.Status">
                            <SelectItem TValue="BilliardTableStatus?" Value="null">@L["All"]</SelectItem>
                            @foreach (var status in Enum.GetValues<BilliardTableStatus>())
                            {
                                <SelectItem TValue="BilliardTableStatus?" Value="status">
                                    @L[$"Enum:BilliardTableStatus.{status}"]
                                </SelectItem>
                            }
                        </Select>
                    </Field>
                </Column>
                <Column ColumnSize="ColumnSize.IsAuto">
                    <Field>
                        <FieldLabel>&nbsp;</FieldLabel>
                        <Button Color="Color.Primary" Clicked="GetBilliardTablesAsync">
                            @L["Search"]
                        </Button>
                    </Field>
                </Column>
            </Row>
        </Form>
        
        @* 数据网格 *@
        <DataGrid TItem="BilliardTableDto"
                  Data="BilliardTableList"
                  ReadData="OnDataGridReadAsync"
                  TotalItems="TotalCount"
                  ShowPager="true"
                  PageSize="PageSize"
                  CurrentPage="CurrentPage"
                  Responsive="true"
                  Class="text-nowrap">
            <DataGridColumns>
                <DataGridColumn TItem="BilliardTableDto"
                               Field="Number"
                               Caption="@L["TableNumber"]"
                               Sortable="true" />
                
                <DataGridColumn TItem="BilliardTableDto"
                               Field="Type"
                               Caption="@L["Type"]"
                               Sortable="true">
                    <DisplayTemplate>
                        @L[$"Enum:BilliardTableType.{context.Type}"]
                    </DisplayTemplate>
                </DataGridColumn>
                
                <DataGridColumn TItem="BilliardTableDto"
                               Field="Status"
                               Caption="@L["Status"]"
                               Sortable="true">
                    <DisplayTemplate>
                        <Badge Color="@GetStatusColor(context.Status)">
                            @L[$"Enum:BilliardTableStatus.{context.Status}"]
                        </Badge>
                    </DisplayTemplate>
                </DataGridColumn>
                
                <DataGridColumn TItem="BilliardTableDto"
                               Field="HourlyRate"
                               Caption="@L["HourlyRate"]"
                               Sortable="true">
                    <DisplayTemplate>
                        ¥@context.HourlyRate.ToString("F2")
                    </DisplayTemplate>
                </DataGridColumn>
                
                <DataGridEntityActionsColumn TItem="BilliardTableDto" @ref="EntityActionsColumn">
                    <DisplayTemplate>
                        <EntityActions TItem="BilliardTableDto" EntityActionsColumn="EntityActionsColumn">
                            <EntityAction TItem="BilliardTableDto"
                                        Text="@L["Edit"]"
                                        Visible="HasUpdatePermission"
                                        Clicked="() => OpenEditModalAsync(context)" />
                            <EntityAction TItem="BilliardTableDto"
                                        Text="@L["Delete"]"
                                        Visible="HasDeletePermission"
                                        Clicked="() => DeleteEntityAsync(context)"
                                        ConfirmationMessage="() => GetDeleteConfirmationMessage(context)" />
                        </EntityActions>
                    </DisplayTemplate>
                </DataGridEntityActionsColumn>
            </DataGridColumns>
        </DataGrid>
    </CardBody>
</Card>

@* 模态对话框 *@
<BilliardTableCreateModal @ref="CreateBilliardTableModal"
                         Created="GetBilliardTablesAsync" />

<BilliardTableEditModal @ref="EditBilliardTableModal"
                       Updated="GetBilliardTablesAsync" />
```

### 2. 组件代码后置

```csharp
// BilliardTableList.razor.cs
public partial class BilliardTableList
{
    // 注入服务
    [Inject] protected IBilliardTableAppService BilliardTableAppService { get; set; }
    [Inject] protected IDataGridEntityActionsColumn<BilliardTableDto> EntityActionsColumn { get; set; }

    // 数据属性
    protected IReadOnlyList<BilliardTableDto> BilliardTableList { get; set; } = new List<BilliardTableDto>();
    protected int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
    protected int CurrentPage { get; set; } = 1;
    protected string CurrentSorting { get; set; } = string.Empty;
    protected int TotalCount { get; set; }
    
    // 过滤器
    protected GetBilliardTablesInput Filter { get; set; }
    
    // 模态对话框
    protected BilliardTableCreateModal CreateBilliardTableModal { get; set; } = new();
    protected BilliardTableEditModal EditBilliardTableModal { get; set; } = new();

    // 权限检查
    protected bool HasCreatePermission { get; set; }
    protected bool HasUpdatePermission { get; set; }
    protected bool HasDeletePermission { get; set; }

    // 构造函数
    public BilliardTableList()
    {
        Filter = new GetBilliardTablesInput
        {
            MaxResultCount = PageSize,
            SkipCount = (CurrentPage - 1) * PageSize,
            Sorting = CurrentSorting
        };
        
        LocalizationResource = typeof(BilliardHallResource);
    }

    // 生命周期方法
    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
        await GetBilliardTablesAsync();
    }

    // 权限设置
    protected virtual async Task SetPermissionsAsync()
    {
        HasCreatePermission = await AuthorizationService.IsGrantedAsync(BilliardHallPermissions.BilliardTables.Create);
        HasUpdatePermission = await AuthorizationService.IsGrantedAsync(BilliardHallPermissions.BilliardTables.Edit);
        HasDeletePermission = await AuthorizationService.IsGrantedAsync(BilliardHallPermissions.BilliardTables.Delete);
    }

    // 数据加载
    protected virtual async Task GetBilliardTablesAsync()
    {
        Filter.MaxResultCount = PageSize;
        Filter.SkipCount = (CurrentPage - 1) * PageSize;
        Filter.Sorting = CurrentSorting;

        var result = await BilliardTableAppService.GetListAsync(Filter);
        BilliardTableList = result.Items;
        TotalCount = (int)result.TotalCount;
    }

    // 数据网格事件
    protected virtual async Task OnDataGridReadAsync(DataGridReadDataEventArgs<BilliardTableDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");
        CurrentPage = e.Page;

        await GetBilliardTablesAsync();
        await InvokeAsync(StateHasChanged);
    }

    // 模态对话框操作
    protected virtual async Task OpenCreateModalAsync()
    {
        await CreateBilliardTableModal.OpenAsync();
    }

    protected virtual async Task OpenEditModalAsync(BilliardTableDto entity)
    {
        await EditBilliardTableModal.OpenAsync(entity.Id);
    }

    // 删除操作
    protected virtual async Task DeleteEntityAsync(BilliardTableDto entity)
    {
        await BilliardTableAppService.DeleteAsync(entity.Id);
        await GetBilliardTablesAsync();
    }

    protected virtual string GetDeleteConfirmationMessage(BilliardTableDto entity)
    {
        return L["DeletionConfirmationMessage", entity.Number];
    }

    // 辅助方法
    protected Color GetStatusColor(BilliardTableStatus status)
    {
        return status switch
        {
            BilliardTableStatus.Available => Color.Success,
            BilliardTableStatus.Occupied => Color.Danger,
            BilliardTableStatus.Reserved => Color.Warning,
            BilliardTableStatus.Maintenance => Color.Secondary,
            BilliardTableStatus.OutOfOrder => Color.Dark,
            _ => Color.Light
        };
    }
}
```

### 3. 模态对话框组件

```razor
@* BilliardTableCreateModal.razor *@
@using Zss.BilliardHall.BilliardTables
@inherits BilliardHallComponentBase
@inject IBilliardTableAppService BilliardTableAppService

<Modal @ref="CreateModalRef">
    <ModalContent Centered="true" Size="ModalSize.Large">
        <Form id="BilliardTableCreateForm">
            <ModalHeader>
                <ModalTitle>@L["NewBilliardTable"]</ModalTitle>
                <CloseButton />
            </ModalHeader>
            <ModalBody>
                <Validations @ref="CreateValidationsRef"
                           Model="@NewEntity"
                           ValidateOnLoad="false">
                    
                    <Validation MessageLocalizer="@LH.Localize">
                        <Field>
                            <FieldLabel>@L["TableNumber"] *</FieldLabel>
                            <NumericEdit TValue="int" @bind-Value="@NewEntity.Number">
                                <Feedback>
                                    <ValidationError />
                                </Feedback>
                            </NumericEdit>
                        </Field>
                    </Validation>

                    <Validation MessageLocalizer="@LH.Localize">
                        <Field>
                            <FieldLabel>@L["Type"] *</FieldLabel>
                            <Select TValue="BilliardTableType" @bind-SelectedValue="@NewEntity.Type">
                                @foreach (var type in Enum.GetValues<BilliardTableType>())
                                {
                                    <SelectItem TValue="BilliardTableType" Value="type">
                                        @L[$"Enum:BilliardTableType.{type}"]
                                    </SelectItem>
                                }
                            </Select>
                        </Field>
                    </Validation>

                    <Validation MessageLocalizer="@LH.Localize">
                        <Field>
                            <FieldLabel>@L["HourlyRate"] *</FieldLabel>
                            <NumericEdit TValue="decimal" @bind-Value="@NewEntity.HourlyRate" Decimals="2">
                                <Feedback>
                                    <ValidationError />
                                </Feedback>
                            </NumericEdit>
                        </Field>
                    </Validation>

                    <Row>
                        <Column ColumnSize="ColumnSize.Is6">
                            <Validation MessageLocalizer="@LH.Localize">
                                <Field>
                                    <FieldLabel>@L["LocationX"]</FieldLabel>
                                    <NumericEdit TValue="float" @bind-Value="@NewEntity.LocationX" />
                                </Field>
                            </Validation>
                        </Column>
                        <Column ColumnSize="ColumnSize.Is6">
                            <Validation MessageLocalizer="@LH.Localize">
                                <Field>
                                    <FieldLabel>@L["LocationY"]</FieldLabel>
                                    <NumericEdit TValue="float" @bind-Value="@NewEntity.LocationY" />
                                </Field>
                            </Validation>
                        </Column>
                    </Row>

                </Validations>
            </ModalBody>
            <ModalFooter>
                <Button Color="Color.Secondary" Clicked="CloseCreateModalAsync">
                    @L["Cancel"]
                </Button>
                <SubmitButton Form="BilliardTableCreateForm" Clicked="CreateEntityAsync">
                    @L["Save"]
                </SubmitButton>
            </ModalFooter>
        </Form>
    </ModalContent>
</Modal>

@code {
    [Parameter] public EventCallback Created { get; set; }

    protected Modal CreateModalRef { get; set; } = new();
    protected Validations CreateValidationsRef { get; set; } = new();
    protected CreateBilliardTableDto NewEntity { get; set; } = new();

    public virtual async Task OpenAsync()
    {
        try
        {
            await CreateValidationsRef.ClearAll();
            
            NewEntity = new CreateBilliardTableDto();
            await InvokeAsync(CreateModalRef.Show);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    protected virtual async Task CloseCreateModalAsync()
    {
        NewEntity = new CreateBilliardTableDto();
        await InvokeAsync(CreateModalRef.Hide);
    }

    protected virtual async Task CreateEntityAsync()
    {
        try
        {
            var validate = await CreateValidationsRef.ValidateAll();
            if (!validate)
            {
                return;
            }

            await BilliardTableAppService.CreateAsync(NewEntity);
            
            await Created.InvokeAsync();
            await CloseCreateModalAsync();
            
            await Notify.Success(L["SuccessfullyCreated"]);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
```

### 4. 可重用组件

```razor
@* BilliardTableCard.razor - 卡片组件 *@
@using Zss.BilliardHall.BilliardTables
@inherits BilliardHallComponentBase

<Card Class="billiard-table-card h-100">
    <CardImage Source="@GetTableImage()" Alt="@($"Table {BilliardTable.Number}")" />
    <CardHeader>
        <CardTitle Size="5">
            台球桌 #@BilliardTable.Number
        </CardTitle>
        <CardSubtitle>
            <Badge Color="@GetStatusColor(BilliardTable.Status)">
                @L[$"Enum:BilliardTableStatus.{BilliardTable.Status}"]
            </Badge>
        </CardSubtitle>
    </CardHeader>
    <CardBody>
        <CardText>
            <div class="mb-2">
                <Icon Name="IconName.Table" class="me-2" />
                <strong>类型:</strong> @L[$"Enum:BilliardTableType.{BilliardTable.Type}"]
            </div>
            <div class="mb-2">
                <Icon Name="IconName.Money" class="me-2" />
                <strong>价格:</strong> ¥@BilliardTable.HourlyRate.ToString("F2")/小时
            </div>
            @if (ShowLocation)
            {
                <div class="mb-2">
                    <Icon Name="IconName.Location" class="me-2" />
                    <strong>位置:</strong> (@BilliardTable.LocationX, @BilliardTable.LocationY)
                </div>
            }
        </CardText>
    </CardBody>
    <CardFooter>
        <div class="d-flex justify-content-between">
            @if (BilliardTable.Status == BilliardTableStatus.Available && AllowReserve)
            {
                <Button Color="Color.Primary" 
                        Size="Size.Small"
                        Clicked="@(() => OnReserveClicked.InvokeAsync(BilliardTable.Id))"
                        Loading="@IsReserving">
                    <Icon Name="IconName.Calendar" class="me-1" />
                    预约
                </Button>
            }
            
            @if (HasEditPermission)
            {
                <Dropdown Direction="Direction.Up">
                    <DropdownToggle Color="Color.Secondary" Size="Size.Small">
                        <Icon Name="IconName.Settings" />
                    </DropdownToggle>
                    <DropdownMenu>
                        <DropdownItem Clicked="@(() => OnEditClicked.InvokeAsync(BilliardTable.Id))">
                            <Icon Name="IconName.Edit" class="me-2" />
                            编辑
                        </DropdownItem>
                        
                        @if (BilliardTable.Status != BilliardTableStatus.Maintenance)
                        {
                            <DropdownDivider />
                            <DropdownItem Clicked="@(() => ChangeStatusAsync(BilliardTableStatus.Maintenance))">
                                <Icon Name="IconName.Wrench" class="me-2" />
                                设为维护
                            </DropdownItem>
                        }
                        
                        @if (BilliardTable.Status == BilliardTableStatus.Maintenance)
                        {
                            <DropdownDivider />
                            <DropdownItem Clicked="@(() => ChangeStatusAsync(BilliardTableStatus.Available))">
                                <Icon Name="IconName.Check" class="me-2" />
                                设为可用
                            </DropdownItem>
                        }
                    </DropdownMenu>
                </Dropdown>
            }
        </div>
    </CardFooter>
</Card>

@code {
    [Parameter] public BilliardTableDto BilliardTable { get; set; } = new();
    [Parameter] public bool ShowLocation { get; set; } = false;
    [Parameter] public bool AllowReserve { get; set; } = true;
    [Parameter] public bool HasEditPermission { get; set; } = false;
    [Parameter] public EventCallback<Guid> OnReserveClicked { get; set; }
    [Parameter] public EventCallback<Guid> OnEditClicked { get; set; }
    [Parameter] public EventCallback<BilliardTableStatus> OnStatusChanged { get; set; }

    private bool IsReserving { get; set; }

    private Color GetStatusColor(BilliardTableStatus status)
    {
        return status switch
        {
            BilliardTableStatus.Available => Color.Success,
            BilliardTableStatus.Occupied => Color.Danger,
            BilliardTableStatus.Reserved => Color.Warning,
            BilliardTableStatus.Maintenance => Color.Secondary,
            BilliardTableStatus.OutOfOrder => Color.Dark,
            _ => Color.Light
        };
    }

    private string GetTableImage()
    {
        return BilliardTable.Type switch
        {
            BilliardTableType.ChineseEightBall => "/images/chinese-eight-ball.jpg",
            BilliardTableType.AmericanNineBall => "/images/american-nine-ball.jpg",
            BilliardTableType.Snooker => "/images/snooker.jpg",
            _ => "/images/default-table.jpg"
        };
    }

    private async Task ChangeStatusAsync(BilliardTableStatus newStatus)
    {
        try
        {
            await OnStatusChanged.InvokeAsync(newStatus);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
```

### 5. Blazor WebAssembly 客户端模式

```csharp
// Program.cs - Blazor WebAssembly 客户端
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zss.BilliardHall.Blazor.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();
```

### 6. 实时更新 (SignalR Integration)

```csharp
// BilliardTableStatusHub.cs - SignalR Hub
[HubRoute("/signalr-hubs/billiard-table-status")]
public class BilliardTableStatusHub : AbpHub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}

// 在 Blazor 组件中使用 SignalR
@implements IAsyncDisposable
@inject IJSRuntime JSRuntime

@code {
    private HubConnection? hubConnection;

    protected override async Task OnInitializedAsync()
    {
        // 建立 SignalR 连接
        hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/signalr-hubs/billiard-table-status"))
            .Build();

        // 监听状态变化
        hubConnection.On<BilliardTableStatusChangedEto>("TableStatusChanged", async (notification) =>
        {
            await InvokeAsync(async () =>
            {
                // 更新UI
                await GetBilliardTablesAsync();
                StateHasChanged();
            });
        });

        await hubConnection.StartAsync();
        await hubConnection.SendAsync("JoinGroup", "BilliardTableStatus");
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

## Blazorise 组件使用最佳实践

### 1. 响应式布局

```razor
<Container Fluid="true">
    <Row>
        <Column ColumnSize="ColumnSize.Is12.OnMobile.Is6.OnTablet.Is4.OnDesktop">
            <BilliardTableCard BilliardTable="table" />
        </Column>
    </Row>
</Container>
```

### 2. 表单验证

```razor
<Validations @ref="validations" Model="@model" ValidateOnLoad="false">
    <Validation Validator="@ValidationRule.IsNotEmpty">
        <Field>
            <FieldLabel>名称</FieldLabel>
            <TextEdit @bind-Text="@model.Name" />
            <FieldHelp>请输入台球桌名称</FieldHelp>
        </Field>
    </Validation>
</Validations>
```

### 3. 数据表格

```razor
<DataGrid TItem="BilliardTableDto"
          Data="@tables"
          Filterable="true"
          FilterMethod="DataGridFilterMethod.StartsWith"
          ShowPager="true"
          PageSize="10">
    <DataGridColumns>
        <DataGridColumn Field="@nameof(BilliardTableDto.Number)" Caption="编号" />
        <DataGridColumn Field="@nameof(BilliardTableDto.Status)" Caption="状态" />
    </DataGridColumns>
</DataGrid>
```

这些模式确保了 Blazor 组件的一致性、可重用性和性能优化。