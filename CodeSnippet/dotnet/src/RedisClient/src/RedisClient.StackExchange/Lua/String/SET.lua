local command = 'SET'
local key = KEYS[1]
local val = ARGV[1]
local expiry = ARGV[2]
local writeBehavior = ARGV[4]
local get = ARGV[5]

if get == 'GET' then
    if expiry == 'KEEPTTL' then
        if writeBehavior ~= '' then
            return redis.pcall(command, key, val, expiry, writeBehavior, get)
        else
            return redis.pcall(command, key, val, expiry, get)
        end
    elseif expiry ~= '' then
        local expiryVal = ARGV[3]
        if writeBehavior ~= '' then
            return redis.pcall(command, key, val, expiry, expiryVal,
                               writeBehavior, get)
        else
            return redis.pcall(command, key, val, expiry, expiryVal, get)
        end
    else
        if writeBehavior ~= '' then
            return redis.pcall(command, key, val, writeBehavior, get)
        else
            return redis.pcall(command, key, val, get)
        end
    end
else
    if expiry == 'KEEPTTL' then
        if writeBehavior ~= '' then
            return redis.pcall(command, key, val, expiry, writeBehavior)
        else
            return redis.pcall(command, key, val, expiry)
        end
    elseif expiry ~= '' then
        local expiryVal = ARGV[3]
        if writeBehavior ~= '' then
            return redis.pcall(command, key, val, expiry, expiryVal,
                               writeBehavior)
        else
            return redis.pcall(command, key, val, expiry, expiryVal)
        end
    else
        if writeBehavior ~= '' then
            return redis.pcall(command, key, val, writeBehavior)
        else
            return redis.pcall(command, key, val)
        end
    end
end
