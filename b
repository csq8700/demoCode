function Convert-XmlNodeToHashtable {
    param(
        [Parameter(Mandatory=$true)]
        [System.Xml.XmlNode]$XmlNode,
        [switch]$ProcessCurrentNode = $true
    )
    
    $result = @{}
    
    # 1. 首先，处理当前节点的所有属性
    if ($XmlNode.Attributes -and $XmlNode.Attributes.Count -gt 0) {
        foreach ($attr in $XmlNode.Attributes) {
            # 将属性以 "@属性名" 的格式存入哈希表，避免与子元素名冲突
            $result["@$($attr.Name)"] = $attr.Value
        }
    }
    
    # 2. 检查并处理当前节点的直接文本内容（非子元素文本）
    # 获取所有子节点中类型为Text（文本）的节点，并拼接其内容
    $directText = ($XmlNode.ChildNodes | Where-Object { $_.NodeType -eq 'Text' } | ForEach-Object { $_.Value }) -join ''
    if ($directText.Trim() -ne '') {
        $result['#text'] = $directText.Trim()
    }
    
    # 3. 递归处理所有子元素节点
    $childElements = $XmlNode.ChildNodes | Where-Object { $_.NodeType -eq 'Element' }
    if ($childElements.Count -gt 0) {
        # 按元素名分组，处理可能存在的多个同名兄弟元素
        $groupedChildren = $childElements | Group-Object Name
        foreach ($group in $groupedChildren) {
            $nodeName = $group.Name
            if ($group.Count -eq 1) {
                # 单一子元素：递归调用，并将结果作为嵌套哈希表存入
                $result[$nodeName] = Convert-XmlNodeToHashtable -XmlNode $group.Group[0]
            } else {
                # 多个同名子元素：将每个的递归结果存入一个数组
                $result[$nodeName] = @()
                foreach ($childNode in $group.Group) {
                    $result[$nodeName] += Convert-XmlNodeToHashtable -XmlNode $childNode
                }
            }
        }
    }
    
    return $result
}



function ConvertTo-FlatHashtable {
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [System.Xml.XmlNode]$XmlNode
    )

    $result = @{}

    # --- 逻辑1：判断“主值”的来源 ---
    # 情况A：如果节点本身有属性，则将第一个属性值作为该节点的“主值”
    if ($XmlNode.Attributes.Count -gt 0) {
        # 直接将第一个属性的值作为该节点键的值
        $result[$XmlNode.Name] = $XmlNode.Attributes[0].Value
    }
    # 情况B：如果节点没有属性，但有直接的文本内容，则将文本内容作为“主值”
    elseif ($XmlNode.InnerText.Trim() -ne '') {
        $result[$XmlNode.Name] = $XmlNode.InnerText.Trim()
    }
    # 情况C：如果节点既无属性也无直接文本（纯容器节点），则值为一个空哈希表
    else {
        $result[$XmlNode.Name] = @{}
    }

    # --- 逻辑2：处理所有子元素节点 ---
    $childElements = $XmlNode.ChildNodes | Where-Object { $_.NodeType -eq 'Element' }
    if ($childElements.Count -gt 0) {
        # 如果主值是字符串（来自属性或文本），则需将其转换为一个嵌套哈希表
        if ($result[$XmlNode.Name] -is [string]) {
            $result[$XmlNode.Name] = @{ $XmlNode.Name = $result[$XmlNode.Name] }
        }
        # 现在，$result[$XmlNode.Name] 肯定是一个哈希表，用于存放其子元素
        $container = $result[$XmlNode.Name]

        foreach ($child in $childElements) {
            # 递归处理子元素
            $childResult = ConvertTo-FlatHashtable -XmlNode $child
            # 将子元素的处理结果合并到父容器中
            foreach ($key in $childResult.Keys) {
                $container[$key] = $childResult[$key]
            }
        }
    }

    return $result
}



function Get-InnerNodesToHashtable {
    param(
        [Parameter(Mandatory=$true)]
        [System.Xml.XmlNode]$XmlNode
    )

    $result = @{}

    # 只处理该节点的子元素节点，不处理节点自身
    $childElements = $XmlNode.ChildNodes | Where-Object { $_.NodeType -eq 'Element' }
    
    foreach ($child in $childElements) {
        # 对每个子元素调用扁平化函数
        $childResult = ConvertTo-FlatHashtable -XmlNode $child
        
        # 将子元素的结果合并到结果哈希表中
        foreach ($key in $childResult.Keys) {
            $result[$key] = $childResult[$key]
        }
    }

    return $result
}

# 使用方式
$targetNode = $xmlDoc.SelectSingleNode("//ComputerName[@ComputerName='1']")
$innerNodes = Get-InnerNodesToHashtable -XmlNode $targetNode

# 输出结果
$innerNodes | ConvertTo-Json



function ConvertTo-NestedHashtable {
    param(
        [Parameter(Mandatory=$true)]
        [System.Xml.XmlNode]$XmlNode,
        [switch]$IncludeAttributes = $true
    )

    $result = @{}
    
    # 1. 处理当前节点的属性
    if ($IncludeAttributes -and $XmlNode.Attributes.Count -gt 0) {
        foreach ($attr in $XmlNode.Attributes) {
            $result["@$($attr.Name)"] = $attr.Value
        }
    }
    
    # 2. 处理所有子节点
    $childElements = $XmlNode.ChildNodes | Where-Object { $_.NodeType -eq 'Element' }
    
    if ($childElements.Count -eq 0) {
        # 如果没有子元素，直接使用文本内容
        $text = $XmlNode.InnerText.Trim()
        if ($text -ne '') {
            return $text  # 返回纯文本值
        }
        return $result   # 返回可能只包含属性的哈希表
    }
    
    # 3. 递归处理子元素
    foreach ($child in $childElements) {
        $childResult = ConvertTo-NestedHashtable -XmlNode $child -IncludeAttributes:$IncludeAttributes
        
        $childName = $child.Name
        
        # 处理同名子元素（转换为数组）
        if ($result.ContainsKey($childName)) {
            if ($result[$childName] -isnot [array]) {
                # 将现有值转换为数组
                $result[$childName] = @($result[$childName])
            }
            $result[$childName] += $childResult
        } else {
            $result[$childName] = $childResult
        }
    }
    
    return $result
}


function ConvertTo-CleanNestedHashtable {
    param(
        [Parameter(Mandatory=$true)]
        [System.Xml.XmlNode]$XmlNode
    )

    $result = @{}
    
    # 1. 首先，处理当前节点的所有属性（关键：属性名不加 @ 前缀）
    if ($XmlNode.Attributes.Count -gt 0) {
        foreach ($attr in $XmlNode.Attributes) {
            # 直接将属性名和值存入哈希表，例如 ServerInstance: "1"
            $result[$attr.Name] = $attr.Value
        }
    }
    
    # 2. 处理所有子元素节点
    $childElements = $XmlNode.ChildNodes | Where-Object { $_.NodeType -eq 'Element' }
    
    if ($childElements.Count -eq 0) {
        # 如果没有子元素，但节点有直接的文本内容，则将其作为值
        # 注意：如果节点同时有属性，文本内容将作为另一个键值对添加
        $text = $XmlNode.InnerText.Trim()
        if ($text -ne '' -and $XmlNode.Attributes.Count -eq 0) {
            # 当节点没有属性时，文本内容作为节点名的值
            return $text
        } elseif ($text -ne '') {
            # 当节点既有属性又有文本时，文本内容作为一个特殊键（如‘_text’）或忽略
            # 根据你的需求，这里可以选择添加，例如：
            # $result['_text'] = $text
            # 但根据你的示例，通常节点属性或子元素才是主要内容，因此这里选择忽略纯文本
        }
        return $result
    }
    
    # 3. 递归处理子元素
    foreach ($child in $childElements) {
        $childResult = ConvertTo-CleanNestedHashtable -XmlNode $child
        
        $childName = $child.Name
        
        # 处理同名子元素（转换为数组）
        if ($result.ContainsKey($childName)) {
            if ($result[$childName] -isnot [array]) {
                $result[$childName] = @($result[$childName])
            }
            $result[$childName] += $childResult
        } else {
            $result[$childName] = $childResult
        }
    }
    
    return $result
}

