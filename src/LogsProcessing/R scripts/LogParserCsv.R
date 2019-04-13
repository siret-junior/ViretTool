#install.packages("rjson")

filterQueries = function(taskBoundariesMatrix, actions, savedQueries) {
  
  resultQueries = c()
  taskId = 1
  
  closestResetAll = max(as.numeric(taskBoundariesMatrix[1,taskId])- 20000,
                        suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,]))))
  resultActions = actions[,actions[1,] >= closestResetAll]
  
  resultTaskBoundaries = list("ClosestResetAll" = closestResetAll, "ServerStart" = taskBoundariesMatrix[1,taskId],"ServerEnd" = taskBoundariesMatrix[2,taskId],
                              "TaskName" = taskBoundariesMatrix[3,taskId], "TaskId" = taskBoundariesMatrix[4,taskId])
  
  for (queryId in 1:length(savedQueries))
  {
    if (savedQueries[queryId] > taskBoundariesMatrix[2,taskId] && taskId < length(taskBoundariesMatrix[1,])) { #after end
      taskId = taskId + 1
      closestResetAll = max(as.numeric(taskBoundariesMatrix[1,taskId]) - 20000,
                            suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,]))))
      resultActions = resultActions[,resultActions[1,] <= taskBoundariesMatrix[2,taskId-1] | resultActions[1,] >= closestResetAll]
      resultTaskBoundaries = rbind(resultTaskBoundaries, list(closestResetAll,taskBoundariesMatrix[1,taskId],taskBoundariesMatrix[2,taskId],
                                                              taskBoundariesMatrix[3,taskId],taskBoundariesMatrix[4,taskId]))
    }
    
    if (savedQueries[queryId] >= taskBoundariesMatrix[1,taskId] || (!is.infinite(closestResetAll) && savedQueries[queryId] > closestResetAll)) { #earlier than start
      if (savedQueries[queryId] <= taskBoundariesMatrix[2,taskId]) 
      {
        resultQueries = cbind(resultQueries, c(savedQueries[queryId], taskBoundariesMatrix[3,taskId]))
      }
    } 
  }
  
  return(list(resultQueries, resultActions, resultTaskBoundaries))
}

isSame = function(firstList, secondList) {
  if (length(firstList) != length(secondList)) {
    return(FALSE)
  }
  unlistedFirst = unlist(firstList)
  unlistedSecond = unlist(secondList)
  if (length(unlistedFirst) != length(unlistedSecond)) {
    return(FALSE)
  }
  sum = sum(unlistedFirst != unlistedSecond)
  return(ifelse(sum > 0, FALSE, TRUE))
}

getModelsChanged = function(currentQ, previousQ) {
  
  changes = c()
  for (i in c(1:2)) {
    fs = c("formerquery", "latterquery")[i]
    
    if (!isSame(currentQ[["keywordquery"]][[fs]][["synsetgroups"]], previousQ[["keywordquery"]][[fs]][["synsetgroups"]])) {
      changes = append(changes, paste0("KW_",i))
    }
    if (!isSame(currentQ[["colorsketchquery"]][[fs]][["colorsketchellipses"]], previousQ[["colorsketchquery"]][[fs]][["colorsketchellipses"]])) {
      changes = append(changes, paste0("Color_",i))
    }
    if (!isSame(currentQ[["facesketchquery"]][[fs]][["colorsketchellipses"]], previousQ[["facesketchquery"]][[fs]][["colorsketchellipses"]])) {
      changes = append(changes, paste0("Face_",i))
    }
    if (!isSame(currentQ[["textsketchquery"]][[fs]][["colorsketchellipses"]], previousQ[["textsketchquery"]][[fs]][["colorsketchellipses"]])) {
      changes = append(changes, paste0("Text_",i))
    }
    if (!isSame(currentQ[["semanticexamplequery"]][[fs]][["positiveexampleids"]], previousQ[["semanticexamplequery"]][[fs]][["positiveexampleids"]])) {
      changes = append(changes, paste0("Semantic_",i))
    }
    if (!isSame(currentQ[["semanticexamplequery"]][[fs]][["externalimages"]], previousQ[["semanticexamplequery"]][[fs]][["externalimages"]])) {
      changes = append(changes, paste0("External_",i))
    }
  }
  
  return (changes)
}

valueOrEmpty = function(list) {
  return(ifelse(length(list) > 0, list, ""))
}


getEqualKwIds = function(kwLabelRow, keywordLabels) {
  
  kwId = as.character(kwLabelRow[[1]])
  result = c()
  if (kwId != "H") {
    result = c(kwId)
  }
  equalities = unlist(strsplit(as.character(kwLabelRow[[4]]), "#"))
  for (equality in equalities) {
    
    newKwLabelRow = keywordLabels[keywordLabels[,2] == equality,]
    result = append(result, getEqualKwIds(newKwLabelRow, keywordLabels))
  }
  
  return (result)
}


getKwText = function(keywordGroups, keywordLabels) {
  
  parts = c()
  for (synsetGroup in keywordGroups) {
    
    groupIds = sapply(synsetGroup[["synsets"]], function(s) { s[["synsetid"]]})
    localParts = c()
    while(length(groupIds) > 0) {
     
      kwLabelRow = keywordLabels[keywordLabels[,1] == groupIds[1],]
      localParts = append(localParts, as.character(sub("#.*", "", kwLabelRow[[3]])))
      groupIds = groupIds[groupIds %ni% getEqualKwIds(kwLabelRow, keywordLabels)]
    }
    
    parts = append(parts, paste(localParts, collapse = " OR "))
  }
  
  return(paste(parts, collapse = " AND "))
}



library(stringr)
library(rjson)
library(pheatmap)
"%ni%" = Negate("%in%")

baseDir = "d:\\Temp\\Downloads\\database-vbs2019\\"

#read valid tasks
competitionsFile = file(paste0(baseDir, "competitions.db"),open="r")
validTasks = fromJSON(readLines(competitionsFile)[2])[["taskSequence"]]
close(competitionsFile)

taskFile = file(paste0(baseDir, "tasks.db"),open="r")
taskLines = readLines(taskFile)
close(taskFile)
validTasks = validTasks[validTasks %in% sapply(taskLines, function(line) {
  jsonData = fromJSON(line)
  ifelse(is.element(jsonData[["_id"]], validTasks) && !startsWith(jsonData[["type"]], "AVS"), jsonData[["_id"]], "")
})]

actionsFile = file(paste0(baseDir, "actionLogs.db"),open="r")
actionLines = readLines(actionsFile)
close(actionsFile)

submissionsFile = file(paste0(baseDir, "submissions.db"),open="r")
submissionsLines = readLines(submissionsFile)
close(submissionsFile)


#tasks definitions
taskBoundariesMatrix = c()
for (line in taskLines)
{
  jsonData = fromJSON(line)
  if (!is.element(jsonData[["_id"]], validTasks)) {
    next
  }
  taskBoundariesMatrix = cbind(taskBoundariesMatrix, c(jsonData[["startTimeStamp"]], jsonData[["endTimeStamp"]], jsonData[["name"]], jsonData[["_id"]]))
}

taskBoundariesMatrix = taskBoundariesMatrix[,order(taskBoundariesMatrix[1,])]
taskBoundaries = as.vector(taskBoundariesMatrix)

allTransformedQueries = c()
browsingActionsForHeatmap = c()
graphData = c()
solvedTasks = c()
aggrInterval = 10 # 10s

for (member in c(0,1)) {
  
  #actions
  actions = c()
  allActions = c()
  for (line in actionLines)
  {
    actionData = fromJSON(line)
    if (is.null(actionData[["teamId"]]) || actionData[["teamId"]] != 4 || actionData[["memberId"]] != member || length(actionData[["events"]]) <= 0) {
      next
    }
      
    allActions = cbind(allActions, sapply(actionData[["events"]], 
                                          function(event) { c(ifelse(is.null(event[["timestamp"]]), NA, event[["timestamp"]]),
                                                              ifelse(is.null(event[["type"]]), NA, event[["type"]]),
                                                              ifelse(is.null(event[["category"]]), NA, event[["category"]]),
                                                              ifelse(is.null(event[["value"]]), NA, event[["value"]])) }))
    if (is.element(actionData[["taskId"]], validTasks)) {
      actions = cbind(actions, sapply(actionData[["events"]], 
                                      function(event) { c(event[["timestamp"]],event[["type"]],event[["category"]],
                                        ifelse(is.null(event[["value"]]), "NA", event[["value"]]), actionData[["taskId"]]) }))
    }
  }
  
  #submissions
  submissions = c()
  allSubmissions = c()
  for (line in submissionsLines)
  {
    subData = fromJSON(line)
    if (is.null(subData[["teamNumber"]]) || subData[["teamNumber"]] != 4 || !is.element(subData[["taskId"]], validTasks)
    ) {
      next
    }
    
    newSub = c(subData[["timestamp"]], ifelse(is.null(subData[["correct"]]), 0, subData[["correct"]]), subData[["taskId"]])
    allSubmissions = cbind(allSubmissions, newSub)
    
    if (subData[["memberNumber"]] == member) {
      submissions = cbind(submissions, newSub)
    }
    
  }
  
  #saved tasks
  baseSavedQPath = "..\\..\\..\\Logs\\2019-01-09_VBS2019\\"
  allSavedQueries = as.numeric(sub(".json$", "", list.files(paste0(baseSavedQPath, ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"), "\\QueriesLog_sortingModelsUpdated"))))
  
  newQueriesAndActions = filterQueries(taskBoundariesMatrix, actions, allSavedQueries)
  filteredSavedQueries = newQueriesAndActions[[1]]
  actions = newQueriesAndActions[[2]]
  filteredTaskBoundaries = newQueriesAndActions[[3]]
  
  browsingActions = cbind(t(actions[,actions[3,] == "Browsing"]), c(NA), c(NA))
  
  browActTemp = c()
  solvedTasksTemp = c()
  #assign 10s IDS to actions
  for(taskId in validTasks) {
    boundary = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskId"]==taskId,]
    serverStart = as.numeric(boundary[["ServerStart"]])
    firstSucSub = min(as.numeric(allSubmissions[,allSubmissions[3,]==taskId & allSubmissions[2,]=="TRUE"][1]))
    if (!is.na(firstSucSub)) {
      newSolvedTask = c(boundary[["TaskName"]],sapply((0:50),function(n) { ifelse(serverStart + n*aggrInterval*1000 < firstSucSub, 1, 0)} ))
      solvedTasksTemp = rbind(solvedTasksTemp, newSolvedTask)
    }

    browsingActions[browsingActions[,1] >= boundary[["ServerStart"]] & browsingActions[,1] <= boundary[["ServerEnd"]],6:7] =
      cbind(floor((as.numeric(browsingActions[browsingActions[,1] >= boundary[["ServerStart"]] & browsingActions[,1] <= boundary[["ServerEnd"]],1]) - as.numeric(boundary[["ServerStart"]])) / (1000 * aggrInterval)),
        boundary[["TaskName"]])

    # filter for after submission time
    browActTemp = rbind(browActTemp, browsingActions[browsingActions[,1] >= boundary[["ServerStart"]] & browsingActions[,1] <= boundary[["ServerEnd"]]
                                                     & (is.na(firstSucSub) | browsingActions[,1] <= firstSucSub),])
  }
  browsingActions = browActTemp
  solvedTasks[[as.character(member)]] = solvedTasksTemp
  
  write.table(filteredTaskBoundaries, paste0("TaskBoundaries_member_",member,".csv"), row.names = FALSE, sep = ";")
  write.table(t(submissions), paste0("Submissions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  write.table(browsingActions, paste0("Actions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  #task order
  write.table(sapply(validTasks, function(taskId) {filteredTaskBoundaries[filteredTaskBoundaries[,"TaskId"]==taskId,][["TaskName"]]}),
              paste0("Tasks_order_member_",member,".csv"), col.names = TRUE, row.names = FALSE, sep = ";")
  
  queryResultsFile = file(paste0("ResultRankings_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),".txt"),open="r")
  queryResults = unname(sapply(readLines(queryResultsFile),
                        function(line) {
                          queryResJson = fromJSON(line)
                          return (t(c(queryResJson[["queryTimestamp"]], queryResJson[["topVideoPosition"]],queryResJson[["topShotPosition"]])))
                        }))
  close(queryResultsFile)
  
  queryResultsBeforeFilterFile = file(paste0("ResultRankings_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"_BeforeCountFilter.txt"),open="r")
  queryResultsBF = unname(sapply(readLines(queryResultsBeforeFilterFile),
                               function(line) {
                                 queryResJson = fromJSON(line)
                                 return (t(c(queryResJson[["queryTimestamp"]], queryResJson[["topVideoPosition"]],queryResJson[["topShotPosition"]])))
                               }))
  close(queryResultsBeforeFilterFile)
  
  
  timestamps = read.csv(paste0("timestamps_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),".csv"), header = TRUE, sep = ";")
  taskNames = read.csv("tasknames.csv", header = TRUE, sep = ";")
  keywordLabels = read.csv("V3C1-GoogLeNet.label", header = FALSE, sep = "~")
  
  #transform queries
  transformedQueries = c()
  allSavedQueries = allSavedQueries[order(allSavedQueries)]
  previousSimilarity = NULL
  for (queryTS in allSavedQueries) {
    
    queryFile = file(paste0(baseSavedQPath,ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"\\QueriesLog_sortingModelsUpdated\\",queryTS,".json"),open="r")
    queryData = fromJSON(file = queryFile)
    close(queryFile)
    
    mainModel = queryData[["primarytemporalquery"]]
    firstIsMain = mainModel == 0
    first = ifelse(firstIsMain, "formerquery", "latterquery")
    second = ifelse(!firstIsMain, "formerquery", "latterquery")
    similarity = queryData[["bitemporalsimilarityquery"]]
    
    if (!is.element(queryTS, filteredSavedQueries[1,])) {
      previousSimilarity = similarity
      next
    }
    
    taskName = filteredSavedQueries[2,filteredSavedQueries[1,] == queryTS]
    taskId = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["TaskId"]]
    firstSucSub = min(as.numeric(allSubmissions[,allSubmissions[3,]==taskId & allSubmissions[2,]=="TRUE"][1]))
    taskStart = as.numeric(filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["ServerStart"]])
    fileTimes = timestamps[timestamps[,"Filename"] == paste0(queryTS,".json"),]
    
    firstFilter = ifelse(firstIsMain, "formerfilteringquery", "latterfilteringquery")
    secondFilter = ifelse(!firstIsMain, "formerfilteringquery", "latterfilteringquery")
    enabledFilters = c(sapply(c(firstFilter,secondFilter), function(filterQuery) { 
      res = c()
      id = ifelse(filterQuery == firstFilter, 1, 2)
      if (queryData[[filterQuery]][["colorsaturationquery"]][["filterstate"]] != 2) {
        res = c(paste0("ColorSatur_",id,":", queryData[[filterQuery]][["colorsaturationquery"]][["threshold"]]))
      }
      if (queryData[[filterQuery]][["percentofblackquery"]][["filterstate"]] != 2) {
        res = c(res, paste0("BW%_",id,":", queryData[[filterQuery]][["percentofblackquery"]][["threshold"]]))
      }
      if (id == 1 & queryData[[filterQuery]][["countfilteringquery"]][["filterstate"]] != 2) {
        res = c(res, paste0("FramesCount:Vid:", queryData[[filterQuery]][["countfilteringquery"]][["maxpervideo"]],",Shots:",queryData[[filterQuery]][["countfilteringquery"]][["maxpershot"]]))
      }
      return (res)}))
    
    transformedQuery = list(
      "TimeStamp" = queryTS,
      "TimeAfterStart" = (queryTS - taskStart) / 1000,
      "TimeBeforeSubmission" = (firstSucSub - queryTS) / 1000,
      "10sId" = floor((queryTS - taskStart) / (1000 * aggrInterval)),
      "TaskName" = taskName,
      "TaskNameShort" = toString(taskNames[taskNames[,"TaskName"] == taskName,"TaskID"]),
      "TopVideoPosition" = queryResults[,queryResults[1,] == queryTS][2],
      "TopShotPosition" = queryResults[,queryResults[1,] == queryTS][3],
      "TopVideoPositionBeforeFilter" = queryResultsBF[,queryResultsBF[1,] == queryTS][2],
      "TopShotPositionBeforeFilter" = queryResultsBF[,queryResultsBF[1,] == queryTS][3],
      "KW_1" = length(similarity[["keywordquery"]][[first]][["synsetgroups"]]),
      "KW_1_Words" = getKwText(similarity[["keywordquery"]][[first]][["synsetgroups"]],keywordLabels ),
      "KW_1_IDS" = paste(sapply(similarity[["keywordquery"]][[first]][["synsetgroups"]], 
                          function(g) { paste(sapply(g[["synsets"]], function(s) { s[["synsetid"]]}),collapse ="+")}),collapse ="|"),
      "KW_2" = length(similarity[["keywordquery"]][[second]][["synsetgroups"]]),
      "KW_2_Words" = getKwText(similarity[["keywordquery"]][[second]][["synsetgroups"]], keywordLabels),
      "KW_2_IDS" = paste(sapply(similarity[["keywordquery"]][[second]][["synsetgroups"]], 
                                function(g) { paste(sapply(g[["synsets"]], function(s) { s[["synsetid"]]}),collapse ="+")}),collapse ="|"),
      "Color_1" = length(similarity[["colorsketchquery"]][[first]][["colorsketchellipses"]]),
      "Color_2" = length(similarity[["colorsketchquery"]][[second]][["colorsketchellipses"]]),
      "Face_1" = length(similarity[["facesketchquery"]][[first]][["colorsketchellipses"]]),
      "Face_2" = length(similarity[["facesketchquery"]][[second]][["colorsketchellipses"]]),
      "Text_1" = length(similarity[["textsketchquery"]][[first]][["colorsketchellipses"]]),
      "Text_2" = length(similarity[["textsketchquery"]][[second]][["colorsketchellipses"]]),
      "Semantic_1" = ifelse(length(similarity[["semanticexamplequery"]][[first]][["positiveexampleids"]]) > 0, "on",
                            ifelse(length(similarity[["semanticexamplequery"]][[first]][["externalimages"]]) > 0, "external", "off")),
      "Semantic_2" = ifelse(length(similarity[["semanticexamplequery"]][[second]][["positiveexampleids"]]) > 0, "on", 
                            ifelse(length(similarity[["semanticexamplequery"]][[second]][["externalimages"]]) > 0, "external", "off")),
      "SortedBy" = switch(as.numeric(queryData[[ifelse(queryData[["primarytemporalquery"]] == 0, "formerfusionquery", "latterfusionquery")]][["sortingsimilaritymodel"]]) + 1,
                          "Keyword", "ColorSketch", "Face", "Text", "Semantic", "None"),
      "ModelsChanged" = paste(getModelsChanged(similarity, previousSimilarity), collapse = "|"),
      "TaskStart" = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["ServerStart"]],
      "TaskEnd" = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["ServerEnd"]],
      "FirstSuccessfulSubmission" = firstSucSub,
      "IsAfterSuccessfulSubmission" = queryTS > firstSucSub,
      "QueryFileCreated" = valueOrEmpty(fileTimes[["CreatedUnixMiliseconds"]]),
      "QueryFileModified" = valueOrEmpty(fileTimes[["ModifiedUnixMiliseconds"]]),
      "QueryFileModifiedDiff" = valueOrEmpty(fileTimes[["CreatedModifiedDifferenceSeconds"]]),
      "EnabledFilters" = paste0(unlist(enabledFilters), collapse = " ")
    )
    
    enabledModels = c(sapply(c("KW_1","KW_2","Color_1","Color_2","Face_1","Face_2","Text_1","Text_2"),
                             function(var) {return (ifelse(transformedQuery[[var]] > 0, var, ""))}),
                      sapply(c("Semantic_1","Semantic_2"), function(var) {return (ifelse(transformedQuery[[var]] != "off", var, ""))}))
    transformedQuery[["EnabledModels"]] = paste0(enabledModels[enabledModels!=""], collapse = " ")
    
    previousSimilarity = similarity
    transformedQueries = rbind(transformedQueries, transformedQuery)
  }
  
  allTransformedQueries[[as.character(member)]] = transformedQueries
  write.table(transformedQueries, paste0("Queries_member_",member,".csv"), row.names = FALSE, sep = ";")
  
  browsingActionsForHeatmap[[as.character(member)]] = browsingActions
}

####### graphs #######
init.graph = function (file.name, width, height, pointsize) {
  dpi = 72
  pdf(file.name, width = width / dpi, height = height / dpi, pointsize = pointsize)
  par(mar=c(2.9,6,2,2), mgp=c(3.9,0.5,0))
}

graph.heat = function (file.name, data, labCol, main, scale="none", fontsize=12, gaps_row=(1:4)*4, xlab=NULL, width=450, height=300, pointsize = 13.5) {
  
  init.graph(file.name, width, height, pointsize)
  
  tryCatch({
    #heatmap.2(data, scale=scale, Rowv=FALSE, Colv = FALSE, dendrogram="none", labCol=labCol, col=rev(terrain.colors(30)), density.info = "none")
    #heatmap(data, scale=scale, Colv = NA, Rowv = NA, labCol=labCol, col=rev(terrain.colors(30)), xlab="Time from task start (s)", main=main, cexRow=0.8)
    pheatmap(data, scale=scale, cluster_rows=FALSE,cluster_cols=FALSE, labels_col=labCol,
             #col=rev(terrain.colors(30)),
             color=colorRampPalette(c("white","#307de8"))(50),
             xlab="Time from task start (s)", main=main, gaps_row=gaps_row,
             width=width, height=height, fontsize=fontsize, fontsize_col=fontsize-1, cellwidth=11, angle_col=0)
    
  }, finally = {
    dev.off()
  }, error = function(e) {
    print(e)
  })
}

strReverse = function(x) {
  sapply(lapply(strsplit(x, NULL), rev), paste, collapse="")
}

for (taskType in c("Textual","Visual")) {
  
  if (taskType == "Visual") {
    novices = c("TRUE", "FALSE")
  } else {
    novices = c("BOTH")
  }
  
  graphData = NULL
  for (member in c("0","1")) {
    for (novice in novices) {
      
      transformedQueries = allTransformedQueries[[member]]
      filter = (transformedQueries[,"IsAfterSuccessfulSubmission"] == FALSE | is.na(transformedQueries[,"IsAfterSuccessfulSubmission"])) & startsWith(unlist(transformedQueries[,"TaskName"]),taskType)
      filteredActions = browsingActionsForHeatmap[[member]]
      
      if (novice == "TRUE") {
        noviceFilter = endsWith(unlist(transformedQueries[,"TaskName"]),"N")
        actionsFilter = endsWith(filteredActions[,7],"N")
      } else if (novice == "FALSE") {
        noviceFilter = !endsWith(unlist(transformedQueries[,"TaskName"]),"N")
        actionsFilter = !endsWith(filteredActions[,7],"N")
      } else {
        noviceFilter = TRUE
        actionsFilter = TRUE
      }
      
        
      selectedQueries = transformedQueries[filter & noviceFilter,c("10sId","ModelsChanged","SortedBy")]
      # selectedQueries[,"ModelsChanged"] = apply(matrix(unlist(selectedQueries[,c("ModelsChanged","SortedBy")]), ncol = 2, byrow = FALSE),1, function(row) { ifelse(row[1] == "", 
      #                                                                                      switch(row[2], Keyword={"KW_1"}, ColorSketch={"Color_1"}, Face={""}, Text={""}, Semantic={"Semantic_"}, None={""}),
      #                                                                                      row[1])})
      
      matrixForHeatmap = matrix(unlist(selectedQueries[,c("10sId","ModelsChanged")]), ncol = 2, byrow = FALSE)
      matrixForHeatmapExpanded = cbind(as.numeric(matrixForHeatmap[,1]), t(sapply(matrixForHeatmap[,2], function(m) { c(str_count(m,"Face_"),
                                                                                                                        max(str_count(m,"Color_"), str_count(m,"Text_")),
                                                                                                                        max(str_count(m,"Semantic_"), str_count(m,"External_")),
                                                                                                                        str_count(m,"KW_"))})))
      #lines with multiple changes are discarded
      matrixForHeatmapExpanded = matrixForHeatmapExpanded[rowSums(matrixForHeatmapExpanded[,-1]) == 1,]
      
      dataForHeatmap = as.matrix(aggregate(matrixForHeatmapExpanded[,-1], by=list(matrixForHeatmapExpanded[,1]), FUN = sum))
      
      filteredActions = filteredActions[startsWith(filteredActions[,7],taskType) & actionsFilter,]
      browsingActions = as.matrix(aggregate(filteredActions[,6], by=list(as.numeric(filteredActions[,6])), FUN = length))
      
      dataForHeatmap = merge(dataForHeatmap,browsingActions, by="Group.1", all=TRUE)
      colnames(dataForHeatmap) = c("Id",paste0(c("F", "C", "I", "K", "B"), "_", ifelse(novice == "TRUE", "N", "E"), ifelse(member == 0, "2", "1")))
      
      if (is.null(graphData)) {
        graphData = dataForHeatmap
      } else {
        graphData = merge(graphData, dataForHeatmap, by="Id", all=TRUE) 
      }
      
    }
  }
  
  graphData[is.na(graphData)] = 0
  graphData = t(graphData)
  xlab = graphData[1,]*10
  if (taskType=="Textual") {
    xlab[!(0:(length(xlab)-1) %% 3 == 0)] = ""
  } else{
    xlab[(1:length(xlab)) %% 2 == 0] = ""
  }
  
  browseGraphData = graphData[startsWith(rownames(graphData),"B_"),]
  browseGraphData = browseGraphData[order(strReverse(rownames(browseGraphData))),]
  graph.heat(paste0(taskType,"_browse.pdf"), browseGraphData, xlab, main=paste0(taskType," KIS browsing interactions"), gaps_row=c(),
             width=ifelse(taskType=="Textual",680,450), height=200, fontsize=ifelse(taskType=="Textual",18.1,12))
  csvData = t(rbind(graphData[1,]*10,browseGraphData))
  colnames(csvData)[1] = "Time interval"
  write.table(csvData, paste0(taskType,"_browsing.csv"), row.names = FALSE, sep = ";")
  
  queryGraphData = graphData[!startsWith(rownames(graphData),"B_"),][-1,]
  queryGraphData = queryGraphData[order(strReverse(rownames(queryGraphData))),]
  graph.heat(paste0(taskType,"_queries.pdf"), queryGraphData, xlab, main=paste0(taskType," KIS query interactions"), gaps_row=(1:ifelse(taskType=="Textual",1,3))*4,
             width=ifelse(taskType=="Textual",680,450), fontsize=ifelse(taskType=="Textual",18.1,12))
  
  csvData = t(rbind(graphData[1,]*10,queryGraphData))
  colnames(csvData)[1] = "Time interval"
  write.table(csvData, paste0(taskType,"_queries.csv"), row.names = FALSE, sep = ";")
}



